import { useCallback, useState } from 'react'
import { useLandParcel, useLandParcels, useRegisterLandParcel, useUpdateLandParcelStatus } from '../hooks/landParcels'
import { ACQUISITION_METHODS, PARCEL_STATUSES, statusLabel, type RegisterLandParcelRequest } from '../hooks/types'
import Pagination from '../components/Pagination'
import SlideOver from '../components/SlideOver'
import StatusBadge from '../components/StatusBadge'

type Panel = 'detail' | 'register' | null

const emptyForm = (): RegisterLandParcelRequest => ({
  legalParcelNumber: '',
  address: '',
  areaAcres: 0,
  acquisitionDate: new Date().toISOString().slice(0, 10),
  acquisitionMethod: 'Purchase',
  covenantTerms: '',
})

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

export default function LandParcelsPage() {
  const [page,         setPage]         = useState(1)
  const [statusFilter, setStatusFilter] = useState('')
  const [selectedId,   setSelectedId]   = useState<string | null>(null)
  const [panel,        setPanel]        = useState<Panel>(null)
  const [form,         setForm]         = useState<RegisterLandParcelRequest>(emptyForm)
  const [detailStatus, setDetailStatus] = useState('')
  const [toast,        setToast]        = useState<string | null>(null)

  const { data, isLoading, isError } = useLandParcels(page, statusFilter || undefined)
  const { data: detail, isLoading: detailLoading } = useLandParcel(
    panel === 'detail' ? selectedId : null,
  )
  const registerMutation    = useRegisterLandParcel()
  const updateStatusMutation = useUpdateLandParcelStatus()

  const showToast = useCallback((msg: string) => {
    setToast(msg)
    setTimeout(() => setToast(null), 3200)
  }, [])

  const openDetail = (id: string) => {
    setSelectedId(id)
    setDetailStatus('')
    setPanel('detail')
  }

  const openRegister = () => {
    setForm(emptyForm())
    setPanel('register')
  }

  const closePanel = () => {
    setPanel(null)
    setSelectedId(null)
  }

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await registerMutation.mutateAsync({
        ...form,
        areaAcres: Number(form.areaAcres),
        covenantTerms: form.covenantTerms || undefined,
      })
      closePanel()
      showToast('Land parcel registered successfully')
    } catch { /* error shown in form */ }
  }

  const handleStatusUpdate = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedId || !detailStatus) return
    try {
      await updateStatusMutation.mutateAsync({ id: selectedId, newStatus: detailStatus })
      showToast('Status updated')
    } catch { /* error shown inline */ }
  }

  const parcels = data?.items ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Property Registry</div>
          <h1 className="page-header__title">Land Parcels</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount.toLocaleString()} parcel{data.totalCount !== 1 ? 's' : ''} in registry
            </div>
          )}
        </div>
        <button className="btn btn-primary" onClick={openRegister}>
          <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
            <path d="M6.5 1v11M1 6.5h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
          </svg>
          Register Parcel
        </button>
      </div>

      <div className="filter-bar">
        <select
          className="select"
          value={statusFilter}
          onChange={e => { setStatusFilter(e.target.value); setPage(1) }}
        >
          <option value="">All Statuses</option>
          {PARCEL_STATUSES.map(s => (
            <option key={s} value={s}>{statusLabel(s)}</option>
          ))}
        </select>
      </div>

      <div className="registry-table-wrap">
        {isLoading && <SkeletonRows />}

        {isError && (
          <div className="error-state">
            <div className="error-state__title">Could not load parcels</div>
            <div className="error-state__sub">Ensure the API and gateway are running.</div>
          </div>
        )}

        {!isLoading && !isError && parcels.length === 0 && (
          <div className="empty-state">
            <div className="empty-state__glyph">⬡</div>
            <div className="empty-state__title">No parcels registered</div>
            <div className="empty-state__sub">Register your first land parcel to get started.</div>
          </div>
        )}

        {!isLoading && !isError && parcels.length > 0 && (
          <>
            <table className="registry-table">
              <thead>
                <tr>
                  <th>Parcel No.</th>
                  <th>Address</th>
                  <th>Area (ac)</th>
                  <th>Status</th>
                  <th>Registered</th>
                </tr>
              </thead>
              <tbody>
                {parcels.map(p => (
                  <tr
                    key={p.id}
                    onClick={() => openDetail(p.id)}
                    className={selectedId === p.id && panel === 'detail' ? 'row--selected' : ''}
                  >
                    <td><span className="cell-mono">{p.legalParcelNumber}</span></td>
                    <td>{p.address}</td>
                    <td><span className="cell-mono">{Number(p.areaAcres).toFixed(2)}</span></td>
                    <td><StatusBadge status={p.status} /></td>
                    <td><span className="cell-mono">{new Date(p.registeredAt).toLocaleDateString()}</span></td>
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
          title={detail?.legalParcelNumber ?? '…'}
          subtitle={detail?.address}
          onClose={closePanel}
        >
          {detailLoading || !detail ? <SkeletonRows /> : (
            <>
              <div className="detail-grid">
                <div className="detail-field">
                  <div className="detail-field__label">Status</div>
                  <div className="detail-field__value"><StatusBadge status={detail.status} /></div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Area</div>
                  <div className="detail-field__value mono">{Number(detail.areaAcres).toFixed(4)} ac</div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Acquisition Method</div>
                  <div className="detail-field__value">{detail.acquisitionMethod}</div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Acquisition Date</div>
                  <div className="detail-field__value mono">{detail.acquisitionDate}</div>
                </div>
                <div className="detail-field full-span">
                  <div className="detail-field__label">Registered By</div>
                  <div className="detail-field__value mono">{detail.registeredBy}</div>
                </div>
                {detail.covenantTerms && (
                  <div className="detail-field full-span">
                    <div className="detail-field__label">Covenant Terms</div>
                    <div className="detail-field__value">{detail.covenantTerms}</div>
                  </div>
                )}
                <div className="detail-field full-span">
                  <div className="detail-field__label">Parcel ID</div>
                  <div className="detail-field__value"><span className="cell-id">{detail.id}</span></div>
                </div>
                <div className="detail-field full-span">
                  <div className="detail-field__label">Registered At</div>
                  <div className="detail-field__value mono">
                    {new Date(detail.registeredAt).toLocaleString()}
                  </div>
                </div>
              </div>

              <div className="detail-section">
                <div className="detail-section__title">Update Status</div>
                <form onSubmit={handleStatusUpdate}>
                  <div className="status-update">
                    <select
                      className="form-select"
                      value={detailStatus || detail.status}
                      onChange={e => setDetailStatus(e.target.value)}
                    >
                      {PARCEL_STATUSES.map(s => (
                        <option key={s} value={s}>{statusLabel(s)}</option>
                      ))}
                    </select>
                    <button
                      type="submit"
                      className="btn btn-secondary"
                      disabled={
                        updateStatusMutation.isPending ||
                        !detailStatus ||
                        detailStatus === detail.status
                      }
                    >
                      {updateStatusMutation.isPending ? 'Saving…' : 'Apply'}
                    </button>
                  </div>
                  {updateStatusMutation.isError && (
                    <div className="form-error">Failed to update status.</div>
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
          title="Register Land Parcel"
          subtitle="Add a new parcel to the CLT registry"
          onClose={closePanel}
          footer={
            <>
              <button className="btn btn-ghost" onClick={closePanel}>Cancel</button>
              <button
                className="btn btn-primary"
                type="submit"
                form="register-parcel-form"
                disabled={registerMutation.isPending}
              >
                {registerMutation.isPending ? 'Registering…' : 'Register Parcel'}
              </button>
            </>
          }
        >
          <form id="register-parcel-form" onSubmit={handleRegister}>
            <div className="form-grid">
              <div className="form-field full">
                <label className="form-label">Legal Parcel Number</label>
                <input
                  className="form-input"
                  value={form.legalParcelNumber}
                  onChange={e => setForm(f => ({ ...f, legalParcelNumber: e.target.value }))}
                  placeholder="e.g. 123-456-789"
                  required
                />
              </div>
              <div className="form-field full">
                <label className="form-label">Address</label>
                <input
                  className="form-input"
                  value={form.address}
                  onChange={e => setForm(f => ({ ...f, address: e.target.value }))}
                  placeholder="Street address"
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Area (acres)</label>
                <input
                  className="form-input"
                  type="number"
                  step="0.0001"
                  min="0.0001"
                  value={form.areaAcres || ''}
                  onChange={e => setForm(f => ({ ...f, areaAcres: parseFloat(e.target.value) || 0 }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Acquisition Date</label>
                <input
                  className="form-input"
                  type="date"
                  value={form.acquisitionDate}
                  onChange={e => setForm(f => ({ ...f, acquisitionDate: e.target.value }))}
                  required
                />
              </div>
              <div className="form-field full">
                <label className="form-label">Acquisition Method</label>
                <select
                  className="form-select"
                  value={form.acquisitionMethod}
                  onChange={e => setForm(f => ({ ...f, acquisitionMethod: e.target.value }))}
                >
                  {ACQUISITION_METHODS.map(m => <option key={m} value={m}>{m}</option>)}
                </select>
              </div>
              <div className="form-field full">
                <label className="form-label">Covenant Terms (optional)</label>
                <textarea
                  className="form-textarea"
                  value={form.covenantTerms}
                  onChange={e => setForm(f => ({ ...f, covenantTerms: e.target.value }))}
                  placeholder="Any restrictions or covenant conditions…"
                />
              </div>
            </div>
            {registerMutation.isError && (
              <div className="form-error">Registration failed. Is the API running?</div>
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
