import { useCallback, useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useBuilding, useBuildings, useRecordAssessment, useRegisterBuilding, useUpdateBuildingStatus } from '../hooks/buildings'
import {
  BUILDING_STATUSES,
  BUILDING_TYPE_LABELS,
  BUILDING_TYPES,
  statusLabel,
  type RecordBuildingAssessmentRequest,
  type RegisterBuildingRequest,
} from '../hooks/types'
import EntityCombobox from '../components/EntityCombobox'
import NavIdLink from '../components/NavIdLink'
import Pagination from '../components/Pagination'
import SlideOver from '../components/SlideOver'
import StatusBadge from '../components/StatusBadge'
import { LandParcelsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import { getAvailableActions } from '../lib/transitionActions'
import { trackTransitionEvent } from '../lib/telemetry'

const landParcelsApi = new LandParcelsApi(apiConfig)

type Panel = 'detail' | 'register' | null

const emptyForm = (): RegisterBuildingRequest => ({
  landParcelId: '',
  buildingType: 'SingleFamily',
  bedrooms: 3,
  bathrooms: 2,
  squareFootage: 1200,
  yearBuilt: new Date().getFullYear(),
  initialAssessedValue: 150000,
})

const emptyAssessment = (): RecordBuildingAssessmentRequest => ({
  assessedValue: 0,
  assessmentDate: new Date().toISOString().slice(0, 10),
  conditionNotes: '',
})

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const fmtNumber = (n: number) => new Intl.NumberFormat('en-US').format(n)

export default function BuildingsPage() {
  const location = useLocation()
  const navigate = useNavigate()

  const [page,           setPage]           = useState(1)
  const [statusFilter,   setStatusFilter]   = useState('')
  const [parcelFilter,   setParcelFilter]   = useState('')
  const [selectedId,     setSelectedId]     = useState<string | null>(null)
  const [panel,          setPanel]          = useState<Panel>(null)
  const [form,           setForm]           = useState<RegisterBuildingRequest>(emptyForm)
  const [assessment,     setAssessment]     = useState<RecordBuildingAssessmentRequest>(emptyAssessment)
  const [pendingActionId, setPendingActionId] = useState<string | null>(null)
  const [toast,          setToast]          = useState<string | null>(null)

  const { data, isLoading, isError } = useBuildings(page, parcelFilter || undefined, statusFilter || undefined)
  const { data: detail, isLoading: detailLoading } = useBuilding(
    panel === 'detail' ? selectedId : null,
  )
  const registerMutation    = useRegisterBuilding()
  const assessmentMutation  = useRecordAssessment()
  const updateStatusMutation = useUpdateBuildingStatus()

  const showToast = useCallback((msg: string) => {
    setToast(msg)
    setTimeout(() => setToast(null), 3200)
  }, [])

  const openDetail = (id: string) => {
    setSelectedId(id)
    setAssessment(emptyAssessment())
    setPanel('detail')
  }

  const openRegister = () => {
    setForm(emptyForm())
    setPanel('register')
  }

  useEffect(() => {
    const openId = (location.state as { openId?: string } | null)?.openId
    if (!openId) {
      return
    }

    openDetail(openId)
    navigate(
      { pathname: location.pathname, search: location.search, hash: location.hash },
      { replace: true, state: null },
    )
  }, [location, navigate])

  const closePanel = () => {
    setPanel(null)
    setSelectedId(null)
  }

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await registerMutation.mutateAsync({
        ...form,
        bedrooms:             Number(form.bedrooms),
        bathrooms:            Number(form.bathrooms),
        squareFootage:        Number(form.squareFootage),
        yearBuilt:            Number(form.yearBuilt),
        initialAssessedValue: Number(form.initialAssessedValue),
      })
      closePanel()
      showToast('Building registered successfully')
    } catch { /* error shown in form */ }
  }

  const handleAssessment = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedId) return
    try {
      await assessmentMutation.mutateAsync({
        buildingId: selectedId,
        req: {
          ...assessment,
          assessedValue: Number(assessment.assessedValue),
          conditionNotes: assessment.conditionNotes || undefined,
        },
      })
      setAssessment(emptyAssessment())
      showToast('Assessment recorded')
    } catch { /* error shown inline */ }
  }

  const handleTransitionAction = async (action: { id: string; label: string; toState?: string; requiresConfirmation?: boolean; confirmationMessage?: string }) => {
    if (!selectedId || !detail?.status || !action.toState) return

    if (action.requiresConfirmation && !window.confirm(action.confirmationMessage ?? 'Are you sure?')) {
      return
    }

    trackTransitionEvent({
      event: 'ui.transition',
      aggregate: 'building',
      actionId: action.id,
      entityId: selectedId,
      fromState: detail.status,
      toState: action.toState,
      outcome: 'attempt',
    })

    setPendingActionId(action.id)

    try {
      await updateStatusMutation.mutateAsync({ id: selectedId, newStatus: action.toState })
      showToast(`${action.label} complete`)
      trackTransitionEvent({
        event: 'ui.transition',
        aggregate: 'building',
        actionId: action.id,
        entityId: selectedId,
        fromState: detail.status,
        toState: action.toState,
        outcome: 'success',
      })
    } catch (error) {
      trackTransitionEvent({
        event: 'ui.transition',
        aggregate: 'building',
        actionId: action.id,
        entityId: selectedId,
        fromState: detail.status,
        toState: action.toState,
        outcome: 'failure',
        error: error instanceof Error ? error.message : 'Unknown error',
      })
    } finally {
      setPendingActionId(null)
    }
  }

  const clearParcelFilter = () => {
    setParcelFilter('')
    setPage(1)
  }

  const buildings = data?.items ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Property Registry</div>
          <h1 className="page-header__title">Buildings</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount.toLocaleString()} building{data.totalCount !== 1 ? 's' : ''} in registry
            </div>
          )}
        </div>
        <button className="btn btn-primary" onClick={openRegister}>
          <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
            <path d="M6.5 1v11M1 6.5h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
          </svg>
          Register Building
        </button>
      </div>

      <div className="filter-bar">
        <select
          className="select"
          value={statusFilter}
          onChange={e => { setStatusFilter(e.target.value); setPage(1) }}
        >
          <option value="">All Statuses</option>
          {BUILDING_STATUSES.map(s => (
            <option key={s} value={s}>{statusLabel(s)}</option>
          ))}
        </select>

        <div style={{ display: 'flex', gap: 6, alignItems: 'center' }}>
          <EntityCombobox
            value={parcelFilter || null}
            onChange={id => { setParcelFilter(id ?? ''); setPage(1) }}
            fetchOptions={async q => {
              const result = await landParcelsApi.getLandParcels({ search: q, pageSize: 10 })
              return (result.items ?? []).map(p => ({
                id: p.id!,
                label: [p.legalParcelNumber, p.address].filter(Boolean).join(' — '),
              }))
            }}
            placeholder="Filter by parcel…"
          />
          {parcelFilter && (
            <button type="button" className="btn btn-ghost btn-sm" onClick={clearParcelFilter}>
              Clear
            </button>
          )}
        </div>
      </div>

      <div className="registry-table-wrap">
        {isLoading && <SkeletonRows />}

        {isError && (
          <div className="error-state">
            <div className="error-state__title">Could not load buildings</div>
            <div className="error-state__sub">Ensure the API and gateway are running.</div>
          </div>
        )}

        {!isLoading && !isError && buildings.length === 0 && (
          <div className="empty-state">
            <div className="empty-state__glyph">⌂</div>
            <div className="empty-state__title">No buildings registered</div>
            <div className="empty-state__sub">Register a building on an existing land parcel.</div>
          </div>
        )}

        {!isLoading && !isError && buildings.length > 0 && (
          <>
            <table className="registry-table">
              <thead>
                <tr>
                  <th>Type</th>
                  <th>Beds / Baths</th>
                  <th>Sq. Ft.</th>
                  <th>Assessed Value</th>
                  <th>Status</th>
                  <th>Parcel</th>
                </tr>
              </thead>
              <tbody>
                {buildings.map(b => (
                  <tr
                    key={b.id}
                    onClick={() => openDetail(b.id)}
                    className={selectedId === b.id && panel === 'detail' ? 'row--selected' : ''}
                  >
                    <td>{BUILDING_TYPE_LABELS[b.buildingType] ?? b.buildingType}</td>
                    <td>
                      <span className="cell-mono">
                        {b.bedrooms} bd / {Number(b.bathrooms).toFixed(1)} ba
                      </span>
                    </td>
                    <td><span className="cell-mono">{fmtNumber(Number(b.squareFootage))}</span></td>
                    <td><span className="cell-mono">{fmtCurrency(Number(b.assessedValue))}</span></td>
                    <td><StatusBadge status={b.status} /></td>
                    <td>
                      <span className="cell-id" title={b.landParcelId}>
                        {b.landParcelId.slice(0, 8)}…
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {data && data.totalPages > 1 && (
              <Pagination
                page={page}
                totalPages={Number(data.totalPages)}
                totalCount={Number(data.totalCount)}
                pageSize={20}
                onPage={setPage}
              />
            )}
          </>
        )}
      </div>

      {/* ── Detail panel ─── */}
      {panel === 'detail' && (
        <SlideOver
          title={detail ? (BUILDING_TYPE_LABELS[detail.buildingType] ?? detail.buildingType) : '…'}
          subtitle={detail ? `${detail.bedrooms} bd · ${Number(detail.bathrooms).toFixed(1)} ba · ${fmtNumber(Number(detail.squareFootage))} sq ft` : undefined}
          onClose={closePanel}
        >
          {detailLoading || !detail ? <SkeletonRows /> : (
            <>
              {(() => {
                const actions = getAvailableActions('building', detail.status)
                return actions.length > 0 ? (
                  <div className="detail-section">
                    <div className="detail-section__title">Available Actions</div>
                    <div className="status-update" style={{ flexWrap: 'wrap', gap: 8 }}>
                      {actions.map(action => (
                        <button
                          key={action.id}
                          type="button"
                          className="btn btn-secondary"
                          onClick={() => handleTransitionAction(action)}
                          disabled={updateStatusMutation.isPending}
                        >
                          {updateStatusMutation.isPending && pendingActionId === action.id ? 'Saving…' : action.label}
                        </button>
                      ))}
                    </div>
                    {updateStatusMutation.isError && (
                      <div className="form-error">Failed to update status.</div>
                    )}
                  </div>
                ) : null
              })()}

              <div className="detail-grid">
                <div className="detail-field">
                  <div className="detail-field__label">Status</div>
                  <div className="detail-field__value"><StatusBadge status={detail.status} /></div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Year Built</div>
                  <div className="detail-field__value mono">{detail.yearBuilt}</div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Assessed Value</div>
                  <div className="detail-field__value mono">{fmtCurrency(Number(detail.assessedValue))}</div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Last Assessed</div>
                  <div className="detail-field__value mono">{detail.lastAssessedDate ?? '—'}</div>
                </div>
                {detail.conditionNotes && (
                  <div className="detail-field full-span">
                    <div className="detail-field__label">Condition Notes</div>
                    <div className="detail-field__value">{detail.conditionNotes}</div>
                  </div>
                )}
                <div className="detail-field full-span">
                  <div className="detail-field__value">
                    <NavIdLink id={detail.landParcelId} to="/land-parcels" label="Open Land Parcel" />
                  </div>
                </div>
                <div className="detail-field full-span">
                  <div className="detail-field__label">Registered At</div>
                  <div className="detail-field__value mono">
                    {new Date(detail.registeredAt).toLocaleString()}
                  </div>
                </div>
              </div>

              <div className="detail-section">
                <div className="detail-section__title">Record Assessment</div>
                <form onSubmit={handleAssessment}>
                  <div className="form-grid" style={{ marginBottom: 12 }}>
                    <div className="form-field">
                      <label className="form-label">Assessed Value ($)</label>
                      <input
                        className="form-input"
                        type="number"
                        min="0"
                        step="1"
                        value={assessment.assessedValue || ''}
                        onChange={e => setAssessment(a => ({ ...a, assessedValue: parseFloat(e.target.value) || 0 }))}
                        required
                      />
                    </div>
                    <div className="form-field">
                      <label className="form-label">Assessment Date</label>
                      <input
                        className="form-input"
                        type="date"
                        value={assessment.assessmentDate}
                        onChange={e => setAssessment(a => ({ ...a, assessmentDate: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="form-field full">
                      <label className="form-label">Condition Notes (optional)</label>
                      <textarea
                        className="form-textarea"
                        value={assessment.conditionNotes}
                        onChange={e => setAssessment(a => ({ ...a, conditionNotes: e.target.value }))}
                        placeholder="Observations from the assessment…"
                      />
                    </div>
                  </div>
                  <button
                    type="submit"
                    className="btn btn-secondary"
                    disabled={assessmentMutation.isPending || !assessment.assessedValue}
                  >
                    {assessmentMutation.isPending ? 'Recording…' : 'Record Assessment'}
                  </button>
                  {assessmentMutation.isError && (
                    <div className="form-error">Failed to record assessment.</div>
                  )}
                </form>
              </div>

            </>
          )}
        </SlideOver>
      )}

      {/* ── Register panel ─── */}
      {panel === 'register' && (
        <SlideOver
          title="Register Building"
          subtitle="Add a new building to an existing land parcel"
          onClose={closePanel}
          footer={
            <>
              <button className="btn btn-ghost" onClick={closePanel}>Cancel</button>
              <button
                className="btn btn-primary"
                type="submit"
                form="register-building-form"
                disabled={registerMutation.isPending || !form.landParcelId}
              >
                {registerMutation.isPending ? 'Registering…' : 'Register Building'}
              </button>
            </>
          }
        >
          <form id="register-building-form" onSubmit={handleRegister}>
            <div className="form-grid">
              <div className="form-field full">
                <label className="form-label">Land Parcel</label>
                <EntityCombobox
                  value={form.landParcelId || null}
                  onChange={id => setForm(f => ({ ...f, landParcelId: id ?? '' }))}
                  fetchOptions={async q => {
                    const result = await landParcelsApi.getLandParcels({ search: q, pageSize: 10 })
                    return (result.items ?? []).map(p => ({
                      id: p.id!,
                      label: [p.legalParcelNumber, p.address].filter(Boolean).join(' — '),
                    }))
                  }}
                  placeholder="Search parcels…"
                />
              </div>
              <div className="form-field full">
                <label className="form-label">Building Type</label>
                <select
                  className="form-select"
                  value={form.buildingType}
                  onChange={e => setForm(f => ({ ...f, buildingType: e.target.value }))}
                >
                  {BUILDING_TYPES.map(t => (
                    <option key={t} value={t}>{BUILDING_TYPE_LABELS[t]}</option>
                  ))}
                </select>
              </div>
              <div className="form-field">
                <label className="form-label">Bedrooms</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  value={form.bedrooms}
                  onChange={e => setForm(f => ({ ...f, bedrooms: parseInt(e.target.value) || 0 }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Bathrooms</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  step="0.5"
                  value={form.bathrooms}
                  onChange={e => setForm(f => ({ ...f, bathrooms: parseFloat(e.target.value) || 0 }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Square Footage</label>
                <input
                  className="form-input"
                  type="number"
                  min="1"
                  value={form.squareFootage}
                  onChange={e => setForm(f => ({ ...f, squareFootage: parseFloat(e.target.value) || 0 }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Year Built</label>
                <input
                  className="form-input"
                  type="number"
                  min="1800"
                  max={new Date().getFullYear()}
                  value={form.yearBuilt}
                  onChange={e => setForm(f => ({ ...f, yearBuilt: parseInt(e.target.value) || 0 }))}
                  required
                />
              </div>
              <div className="form-field full">
                <label className="form-label">Initial Assessed Value ($)</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  step="1"
                  value={form.initialAssessedValue}
                  onChange={e => setForm(f => ({ ...f, initialAssessedValue: parseFloat(e.target.value) || 0 }))}
                  required
                />
              </div>
            </div>
            {registerMutation.isError && (
              <div className="form-error">Registration failed. Check the parcel ID and try again.</div>
            )}
          </form>
        </SlideOver>
      )}

      {toast && (
        <div className="toast toast--success">
          <svg width="15" height="15" viewBox="0 0 15 15" fill="none">
            <circle cx="7.5" cy="7.5" r="6.5" stroke="#4ec48a" strokeWidth="1.4"/>
            <path d="M4.5 7.5l2 2 4-4" stroke="#4ec48a" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round"/>
          </svg>
          {toast}
        </div>
      )}
    </div>
  )
}
