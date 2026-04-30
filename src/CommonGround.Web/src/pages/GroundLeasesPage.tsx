import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import {
  useCreateGroundLease,
  useGroundLease,
  useGroundLeases,
  useRecordStewardshipCheckIn,
} from '../hooks/groundLeases'
import {
  FORMULA_KIND_LABELS,
  FORMULA_KINDS,
  GROUND_LEASE_STATUSES,
  INDEX_TYPES,
  statusLabel,
  type CreateGroundLeaseForm,
  type RecordCheckInForm,
} from '../hooks/types'
import EntityCombobox from '../components/EntityCombobox'
import NavIdLink from '../components/NavIdLink'
import Pagination from '../components/Pagination'
import SlideOver from '../components/SlideOver'
import StatusBadge from '../components/StatusBadge'
import { HomesApi, LandParcelsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import { getErrorMessage } from '../lib/getErrorMessage'
import { useToast } from '../hooks/useToast'

const landParcelsApi = new LandParcelsApi(apiConfig)
const buildingsApi = new HomesApi(apiConfig)

type Panel = 'detail' | 'create' | null

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const fmtPct = (n: number) => `${(n * 100).toFixed(1)}%`

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

const emptyCreate = (): CreateGroundLeaseForm => ({
  landParcelId: '',
  homeId: '',
  leaseStartDate: new Date().toISOString().slice(0, 10),
  termYears: 99,
  covenantTerms: '',
  formulaKind: 'AppraisalBased',
  appreciationShare: 0.25,
  indexType: 'CPI',
  baselineIndexValue: 100,
  annualPercentage: 0.01,
  improvementsCap: 25000,
  hasImprovementsCap: false,
})

const emptyCheckIn = (): RecordCheckInForm => ({
  checkInDate: new Date().toISOString().slice(0, 10),
  notes: '',
})
export default function GroundLeasesPage() {
  const location = useLocation()
  const navigate = useNavigate()

  const [page, setPage] = useState(1)
  const [statusFilter, setStatusFilter] = useState('')
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [panel, setPanel] = useState<Panel>(null)

  const [createForm, setCreateForm] = useState<CreateGroundLeaseForm>(emptyCreate)
  const [checkInForm, setCheckInForm] = useState<RecordCheckInForm>(emptyCheckIn)
  const [createError, setCreateError] = useState<string | null>(null)
  const [checkInError, setCheckInError] = useState<string | null>(null)
  const { success } = useToast()

  const { data, isLoading, isError } = useGroundLeases(page, statusFilter || undefined)
  const { data: detail, isLoading: detailLoading } = useGroundLease(
    panel === 'detail' ? selectedId : null,
  )

  const createMutation = useCreateGroundLease()
  const checkInMutation = useRecordStewardshipCheckIn()

  const openDetail = (id: string) => {
    setSelectedId(id)
    setCheckInForm(emptyCheckIn())
    setCheckInError(null)
    setPanel('detail')
  }

  const openCreate = () => {
    setCreateForm(emptyCreate())
    setCreateError(null)
    setPanel('create')
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

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    setCreateError(null)
    try {
      await createMutation.mutateAsync(createForm)
      closePanel()
      success('Ground lease created')
    } catch (error) {
      setCreateError(await getErrorMessage(error, 'Failed to create ground lease.'))
    }
  }

  const handleCheckIn = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedId) return
    setCheckInError(null)
    try {
      await checkInMutation.mutateAsync({ groundLeaseId: selectedId, form: checkInForm })
      setCheckInForm(emptyCheckIn())
      success('Check-in recorded')
    } catch (error) {
      setCheckInError(await getErrorMessage(error, 'Failed to record check-in.'))
    }
  }

  const leases = data?.items ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Homeownership</div>
          <h1 className="page-header__title">Ground Leases</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} lease{data.totalCount !== 1 ? 's' : ''} in registry
            </div>
          )}
        </div>
        <button className="btn btn-primary" onClick={openCreate}>
          <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
            <path d="M6.5 1v11M1 6.5h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
          </svg>
          Create Lease
        </button>
      </div>

      <div className="filter-bar">
        <select
          className="select"
          value={statusFilter}
          onChange={e => { setStatusFilter(e.target.value); setPage(1) }}
        >
          <option value="">All Statuses</option>
          {GROUND_LEASE_STATUSES.map(s => (
            <option key={s} value={s}>{statusLabel(s)}</option>
          ))}
        </select>
      </div>

      <div className="registry-table-wrap">
        {isLoading && <SkeletonRows />}

        {isError && (
          <div className="error-state">
            <div className="error-state__title">Could not load ground leases</div>
            <div className="error-state__sub">Ensure the API and gateway are running.</div>
          </div>
        )}

        {!isLoading && !isError && leases.length === 0 && (
          <div className="empty-state">
            <div className="empty-state__glyph">⊟</div>
            <div className="empty-state__title">No ground leases found</div>
            <div className="empty-state__sub">Create a ground lease on an existing land parcel and building.</div>
          </div>
        )}

        {!isLoading && !isError && leases.length > 0 && (
          <>
            <table className="registry-table">
              <thead>
                <tr>
                  <th>Homeowner</th>
                  <th>Start Date</th>
                  <th>Term</th>
                  <th>Status</th>
                  <th>Home</th>
                  <th>Parcel</th>
                </tr>
              </thead>
              <tbody>
                {leases.map(l => (
                  <tr
                    key={l.id}
                    onClick={() => openDetail(l.id!)}
                    className={selectedId === l.id && panel === 'detail' ? 'row--selected' : ''}
                  >
                    <td>{l.homeownerFullName ?? <span style={{ color: 'var(--text-dim)' }}>—</span>}</td>
                    <td><span className="cell-mono">{l.leaseStartDate}</span></td>
                    <td><span className="cell-mono">{l.termYears} yr</span></td>
                    <td><StatusBadge status={l.status ?? ''} /></td>
                    <td>
                      <span className="cell-id" title={l.buildingId}>
                        {l.buildingId?.slice(0, 8)}…
                      </span>
                    </td>
                    <td>
                      <span className="cell-id" title={l.landParcelId}>
                        {l.landParcelId?.slice(0, 8)}…
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {data && (data.totalPages ?? 1) > 1 && (
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
          title={detail?.homeownerFullName ?? 'Ground Lease'}
          subtitle={detail ? `${detail.leaseStartDate} · ${detail.termYears} yr · ${statusLabel(detail.status ?? '')}` : undefined}
          onClose={closePanel}
        >
          {detailLoading || !detail ? <SkeletonRows /> : (
            <>
              <div className="detail-grid">
                <div className="detail-field">
                  <div className="detail-field__label">Status</div>
                  <div className="detail-field__value"><StatusBadge status={detail.status ?? ''} /></div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Formula</div>
                  <div className="detail-field__value">{FORMULA_KIND_LABELS[detail.formulaKind ?? ''] ?? detail.formulaKind}</div>
                </div>
                {detail.appreciationShare != null && (
                  <div className="detail-field">
                    <div className="detail-field__label">Appreciation Share</div>
                    <div className="detail-field__value mono">{fmtPct(detail.appreciationShare)}</div>
                  </div>
                )}
                {detail.indexType && (
                  <div className="detail-field">
                    <div className="detail-field__label">Index Type</div>
                    <div className="detail-field__value mono">{detail.indexType}</div>
                  </div>
                )}
                {detail.baselineIndexValue != null && (
                  <div className="detail-field">
                    <div className="detail-field__label">Baseline Index</div>
                    <div className="detail-field__value mono">{detail.baselineIndexValue}</div>
                  </div>
                )}
                {detail.annualPercentage != null && (
                  <div className="detail-field">
                    <div className="detail-field__label">Annual %</div>
                    <div className="detail-field__value mono">{fmtPct(detail.annualPercentage)}</div>
                  </div>
                )}
                {detail.improvementsCap != null && (
                  <div className="detail-field">
                    <div className="detail-field__label">Improvements Cap</div>
                    <div className="detail-field__value mono">{fmtCurrency(detail.improvementsCap)}</div>
                  </div>
                )}
                <div className="detail-field">
                  <div className="detail-field__label">Start Date</div>
                  <div className="detail-field__value mono">{detail.leaseStartDate}</div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Term</div>
                  <div className="detail-field__value mono">{detail.termYears} years</div>
                </div>
                {detail.homeownerEmail && (
                  <div className="detail-field">
                    <div className="detail-field__label">Email</div>
                    <div className="detail-field__value">{detail.homeownerEmail}</div>
                  </div>
                )}
                {detail.homeownerPhone && (
                  <div className="detail-field">
                    <div className="detail-field__label">Phone</div>
                    <div className="detail-field__value mono">{detail.homeownerPhone}</div>
                  </div>
                )}
                {detail.covenantTerms && (
                  <div className="detail-field full-span">
                    <div className="detail-field__label">Covenant Terms</div>
                    <div className="detail-field__value">{detail.covenantTerms}</div>
                  </div>
                )}
                <div className="detail-field full-span">
                  <div className="detail-field__value">
                    <NavIdLink id={detail.buildingId} to="/homes" label="Open Home" />
                  </div>
                </div>
                <div className="detail-field full-span">
                  <div className="detail-field__value">
                    <NavIdLink id={detail.landParcelId} to="/land-parcels" label="Open Land Parcel" />
                  </div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Created</div>
                  <div className="detail-field__value mono">
                    {new Date(detail.createdAt!).toLocaleString()}
                  </div>
                </div>
              </div>

              {/* Stewardship check-ins */}
              <div className="detail-section">
                <div className="detail-section__title">Stewardship Check-Ins</div>
                {(detail.stewardshipCheckIns ?? []).length > 0 && (
                  <div style={{ marginBottom: 14 }}>
                    {(detail.stewardshipCheckIns ?? []).map((c, i) => (
                      <div
                        key={i}
                        style={{
                          padding: '8px 12px',
                          borderLeft: '2px solid var(--amber)',
                          marginBottom: 8,
                          background: 'var(--surface)',
                          borderRadius: '0 4px 4px 0',
                        }}
                      >
                        <div style={{ fontFamily: 'var(--font-mono)', fontSize: 12, color: 'var(--amber)' }}>
                          {c.checkInDate}
                        </div>
                        {c.notes && <div style={{ fontSize: 13, marginTop: 2 }}>{c.notes}</div>}
                        <div style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 2 }}>
                          by {c.recordedBy}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
                <form onSubmit={handleCheckIn}>
                  <div className="form-grid" style={{ marginBottom: 12 }}>
                    <div className="form-field">
                      <label className="form-label">Check-In Date</label>
                      <input
                        className="form-input"
                        type="date"
                        value={checkInForm.checkInDate}
                        onChange={e => setCheckInForm(f => ({ ...f, checkInDate: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="form-field full">
                      <label className="form-label">Notes (optional)</label>
                      <textarea
                        className="form-textarea"
                        value={checkInForm.notes}
                        onChange={e => setCheckInForm(f => ({ ...f, notes: e.target.value }))}
                        placeholder="Observations from the visit…"
                      />
                    </div>
                  </div>
                  <button
                    type="submit"
                    className="btn btn-secondary"
                    disabled={checkInMutation.isPending}
                  >
                    {checkInMutation.isPending ? 'Recording…' : 'Record Check-In'}
                  </button>
                  {checkInError && (
                    <div className="form-error">{checkInError}</div>
                  )}
                </form>
              </div>

              {/* Resale */}
              <div className="detail-section">
                <div className="detail-section__title">Resale</div>
                <button
                  className="btn btn-secondary"
                  onClick={() => navigate('/resale-workflows', { state: { openLeaseId: selectedId } })}
                >
                  Start Resale Workflow
                </button>
              </div>

            </>
          )}
        </SlideOver>
      )}

      {/* ── Create panel ─── */}
      {panel === 'create' && (
        <SlideOver
          title="Create Ground Lease"
          subtitle="Register a new community land trust ground lease"
          onClose={closePanel}
          footer={
            <>
              <button className="btn btn-ghost" onClick={closePanel}>Cancel</button>
              <button
                className="btn btn-primary"
                type="submit"
                form="create-ground-lease-form"
                disabled={createMutation.isPending || !createForm.landParcelId || !createForm.homeId}
              >
                {createMutation.isPending ? 'Creating…' : 'Create Lease'}
              </button>
            </>
          }
        >
          <form id="create-ground-lease-form" onSubmit={handleCreate}>
            <div className="form-grid">
              <div className="form-field full">
                <label className="form-label">Land Parcel</label>
                <EntityCombobox
                  value={createForm.landParcelId || null}
                  onChange={id => setCreateForm(f => ({ ...f, landParcelId: id ?? '', homeId: '' }))}
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
                <label className="form-label">Home</label>
                <EntityCombobox
                  value={createForm.homeId || null}
                  onChange={id => setCreateForm(f => ({ ...f, homeId: id ?? '' }))}
                  fetchOptions={async q => {
                    const result = await buildingsApi.getHomes({ landParcelId: createForm.landParcelId, pageSize: 10 })
                    const lower = q.toLowerCase()
                    const items = result.items ?? []
                    const filtered = lower ? items.filter(b =>
                      `${b.buildingType} ${b.bedrooms} ${b.bathrooms} ${b.squareFootage}`.toLowerCase().includes(lower)
                    ) : items
                    return filtered.map(b => ({
                      id: b.id!,
                      label: `${b.buildingType}, ${b.bedrooms}bd/${b.bathrooms}ba, ${b.squareFootage} sqft`,
                    }))
                  }}
                  placeholder={createForm.landParcelId ? 'Search homes on this parcel…' : 'Select a parcel first'}
                  disabled={!createForm.landParcelId}
                />
              </div>
              <div className="form-field">
                <label className="form-label">Lease Start Date</label>
                <input
                  className="form-input"
                  type="date"
                  value={createForm.leaseStartDate}
                  onChange={e => setCreateForm(f => ({ ...f, leaseStartDate: e.target.value }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Term (years)</label>
                <input
                  className="form-input"
                  type="number"
                  min="1"
                  max="999"
                  value={createForm.termYears}
                  onChange={e => setCreateForm(f => ({ ...f, termYears: parseInt(e.target.value) || 99 }))}
                  required
                />
              </div>
              <div className="form-field full">
                <label className="form-label">Covenant Terms (optional)</label>
                <textarea
                  className="form-textarea"
                  value={createForm.covenantTerms}
                  onChange={e => setCreateForm(f => ({ ...f, covenantTerms: e.target.value }))}
                  placeholder="Describe any covenant restrictions…"
                />
              </div>

              <div className="form-field full">
                <label className="form-label">Resale Formula</label>
                <select
                  className="form-select"
                  value={createForm.formulaKind}
                  onChange={e => setCreateForm(f => ({ ...f, formulaKind: e.target.value }))}
                >
                  {FORMULA_KINDS.map(k => (
                    <option key={k} value={k}>{FORMULA_KIND_LABELS[k]}</option>
                  ))}
                </select>
              </div>

              {createForm.formulaKind === 'AppraisalBased' && (
                <div className="form-field">
                  <label className="form-label">Appreciation Share</label>
                  <input
                    className="form-input"
                    type="number"
                    min="0"
                    max="1"
                    step="0.01"
                    value={createForm.appreciationShare}
                    onChange={e => setCreateForm(f => ({ ...f, appreciationShare: parseFloat(e.target.value) || 0 }))}
                    required
                  />
                  <div className="form-help">Decimal fraction, e.g. 0.25 = 25%</div>
                </div>
              )}

              {createForm.formulaKind === 'IndexBased' && (
                <>
                  <div className="form-field">
                    <label className="form-label">Index Type</label>
                    <select
                      className="form-select"
                      value={createForm.indexType}
                      onChange={e => setCreateForm(f => ({ ...f, indexType: e.target.value }))}
                    >
                      {INDEX_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                    </select>
                  </div>
                  <div className="form-field">
                    <label className="form-label">Baseline Index Value</label>
                    <input
                      className="form-input"
                      type="number"
                      min="0"
                      step="0.01"
                      value={createForm.baselineIndexValue}
                      onChange={e => setCreateForm(f => ({ ...f, baselineIndexValue: parseFloat(e.target.value) || 0 }))}
                      required
                    />
                  </div>
                </>
              )}

              {createForm.formulaKind === 'FixedPercentage' && (
                <div className="form-field">
                  <label className="form-label">Annual Percentage</label>
                  <input
                    className="form-input"
                    type="number"
                    min="0"
                    max="1"
                    step="0.001"
                    value={createForm.annualPercentage}
                    onChange={e => setCreateForm(f => ({ ...f, annualPercentage: parseFloat(e.target.value) || 0 }))}
                    required
                  />
                  <div className="form-help">Decimal fraction, e.g. 0.03 = 3%</div>
                </div>
              )}

              <div className="form-field full" style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                <input
                  type="checkbox"
                  id="has-cap"
                  checked={createForm.hasImprovementsCap}
                  onChange={e => setCreateForm(f => ({ ...f, hasImprovementsCap: e.target.checked }))}
                />
                <label htmlFor="has-cap" className="form-label" style={{ marginBottom: 0 }}>
                  Set improvements cap
                </label>
              </div>

              {createForm.hasImprovementsCap && (
                <div className="form-field">
                  <label className="form-label">Improvements Cap ($)</label>
                  <input
                    className="form-input"
                    type="number"
                    min="0"
                    step="1"
                    value={createForm.improvementsCap}
                    onChange={e => setCreateForm(f => ({ ...f, improvementsCap: parseFloat(e.target.value) || 0 }))}
                    required
                  />
                </div>
              )}
            </div>
            {createError && (
              <div className="form-error">{createError}</div>
            )}
          </form>
        </SlideOver>
      )}

    </div>
  )
}
