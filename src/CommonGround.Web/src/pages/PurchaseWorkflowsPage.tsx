import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import {
  useInitiatePurchaseWorkflow,
  usePurchaseWorkflows,
} from '../hooks/purchaseWorkflows'
import {
  statusLabel,
  WORKFLOW_PHASES,
  type InitiateWorkflowForm,
} from '../hooks/types'
import EntityCombobox from '../components/EntityCombobox'
import Pagination from '../components/Pagination'
import SlideOver from '../components/SlideOver'
import StatusBadge from '../components/StatusBadge'
import { GroundLeasesApi } from '../api'
import { ResponseError } from '../api/runtime'
import { apiConfig } from '../lib/apiConfig'
import PurchaseWorkflowDetailPanel from './purchase-workflows/PurchaseWorkflowDetailPanel'
import { useToast } from '../hooks/useToast'

const groundLeasesApi = new GroundLeasesApi(apiConfig)
const E164_PHONE_PATTERN = /^\+[1-9]\d{1,14}$/

type Panel = 'detail' | 'initiate' | null

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const isValidE164Phone = (value: string) => E164_PHONE_PATTERN.test(value)

async function getErrorMessage(error: unknown, fallback: string) {
  if (!(error instanceof ResponseError)) {
    return fallback
  }

  try {
    const problem = await error.response.clone().json() as {
      detail?: string
      title?: string
      errors?: Record<string, string[]>
    }

    if (problem.detail) {
      return problem.detail
    }

    const firstError = Object.values(problem.errors ?? {}).flat()[0]
    if (firstError) {
      return firstError
    }

    if (problem.title) {
      return problem.title
    }
  } catch {
    return fallback
  }

  return fallback
}

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

const emptyInitiate = (): InitiateWorkflowForm => ({
  groundLeaseId: '',
  buyerFullName: '',
  buyerEmail: '',
  buyerPhone: '',
  purchasePrice: 150000,
})

export default function PurchaseWorkflowsPage() {
  const location = useLocation()
  const navigate = useNavigate()

  const [page, setPage] = useState(1)
  const [phaseFilter, setPhaseFilter] = useState('')
  const [groundLeaseFilter, setGroundLeaseFilter] = useState('')
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [panel, setPanel] = useState<Panel>(null)

  const [initiateForm, setInitiateForm] = useState<InitiateWorkflowForm>(emptyInitiate)

  const [initiateError, setInitiateError] = useState<string | null>(null)
  const { success, error } = useToast()

  const { data, isLoading, isError } = usePurchaseWorkflows(page, groundLeaseFilter || undefined, phaseFilter || undefined)

  const initiateMutation = useInitiatePurchaseWorkflow()

  const showToast = (msg: string, kind: 'success' | 'error' = 'success') => {
    if (kind === 'error') {
      error(msg)
      return
    }

    success(msg)
  }

  const openDetail = (id: string) => {
    setSelectedId(id)
    setPanel('detail')
  }

  const openInitiate = () => {
    setInitiateForm(emptyInitiate())
    setInitiateError(null)
    setPanel('initiate')
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

  const handleInitiate = async (e: React.FormEvent) => {
    e.preventDefault()

    const buyerPhone = initiateForm.buyerPhone.trim()
    if (buyerPhone && !isValidE164Phone(buyerPhone)) {
      setInitiateError('Buyer phone must be in E.164 format, for example +11231231234.')
      return
    }

    setInitiateError(null)

    try {
      await initiateMutation.mutateAsync({
        ...initiateForm,
        buyerPhone,
        purchasePrice: Number(initiateForm.purchasePrice),
      })
      closePanel()
      showToast('Purchase workflow initiated')
    } catch (error) {
      setInitiateError(await getErrorMessage(error, 'Failed to initiate workflow.'))
    }
  }

  const clearGLFilter = () => {
    setGroundLeaseFilter('')
    setPage(1)
  }

  const workflows = data?.items ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Homeownership</div>
          <h1 className="page-header__title">Purchase Workflows</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} workflow{data.totalCount !== 1 ? 's' : ''}
            </div>
          )}
        </div>
        <button className="btn btn-primary" onClick={openInitiate}>
          <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
            <path d="M6.5 1v11M1 6.5h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
          </svg>
          Initiate Workflow
        </button>
      </div>

      <div className="filter-bar">
        <select
          className="select"
          value={phaseFilter}
          onChange={e => { setPhaseFilter(e.target.value); setPage(1) }}
        >
          <option value="">All Phases</option>
          {WORKFLOW_PHASES.map(p => (
            <option key={p} value={p}>{statusLabel(p)}</option>
          ))}
        </select>

        <div style={{ display: 'flex', gap: 6, alignItems: 'center' }}>
          <EntityCombobox
            value={groundLeaseFilter || null}
            onChange={id => { setGroundLeaseFilter(id ?? ''); setPage(1) }}
            fetchOptions={async q => {
              const result = await groundLeasesApi.getGroundLeases({ search: q, pageSize: 10 })
              return (result.items ?? []).map(l => ({
                id: l.id!,
                label: [l.homeownerFullName, (l as { parcelAddress?: string }).parcelAddress]
                  .filter(Boolean).join(' — '),
              }))
            }}
            placeholder="Filter by ground lease…"
          />
          {groundLeaseFilter && (
            <button type="button" className="btn btn-ghost btn-sm" onClick={clearGLFilter}>
              Clear
            </button>
          )}
        </div>
      </div>

      <div className="registry-table-wrap">
        {isLoading && <SkeletonRows />}

        {isError && (
          <div className="error-state">
            <div className="error-state__title">Could not load purchase workflows</div>
            <div className="error-state__sub">Ensure the API and gateway are running.</div>
          </div>
        )}

        {!isLoading && !isError && workflows.length === 0 && (
          <div className="empty-state">
            <div className="empty-state__glyph">⊡</div>
            <div className="empty-state__title">No workflows found</div>
            <div className="empty-state__sub">Initiate a purchase workflow on an active ground lease.</div>
          </div>
        )}

        {!isLoading && !isError && workflows.length > 0 && (
          <>
            <table className="registry-table">
              <thead>
                <tr>
                  <th>Buyer</th>
                  <th>Purchase Price</th>
                  <th>Phase</th>
                  <th>Ground Lease</th>
                  <th>Created</th>
                </tr>
              </thead>
              <tbody>
                {workflows.map(w => (
                  <tr
                    key={w.id}
                    onClick={() => openDetail(w.id!)}
                    className={selectedId === w.id && panel === 'detail' ? 'row--selected' : ''}
                  >
                    <td>{w.buyerFullName}</td>
                    <td><span className="cell-mono">{fmtCurrency(Number(w.purchasePrice))}</span></td>
                    <td><StatusBadge status={w.phase ?? ''} /></td>
                    <td>
                      <span className="cell-id" title={w.groundLeaseId}>
                        {w.groundLeaseId?.slice(0, 8)}…
                      </span>
                    </td>
                    <td><span className="cell-mono">{new Date(w.createdAt!).toLocaleDateString()}</span></td>
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
        <PurchaseWorkflowDetailPanel
          selectedId={selectedId!}
          onClose={closePanel}
          showToast={showToast}
        />
      )}

      {/* ── Initiate panel ─── */}
      {panel === 'initiate' && (
        <SlideOver
          title="Initiate Purchase Workflow"
          subtitle="Start a new purchase process on an active ground lease"
          onClose={closePanel}
          footer={
            <>
              <button className="btn btn-ghost" onClick={closePanel}>Cancel</button>
              <button
                className="btn btn-primary"
                type="submit"
                form="initiate-workflow-form"
                disabled={initiateMutation.isPending || !initiateForm.groundLeaseId}
              >
                {initiateMutation.isPending ? 'Initiating…' : 'Initiate Workflow'}
              </button>
            </>
          }
        >
          <form id="initiate-workflow-form" onSubmit={handleInitiate}>
            <div className="form-grid">
              <div className="form-field full">
                <label className="form-label">Ground Lease</label>
                <EntityCombobox
                  value={initiateForm.groundLeaseId || null}
                  onChange={id => setInitiateForm(f => ({ ...f, groundLeaseId: id ?? '' }))}
                  fetchOptions={async q => {
                    const result = await groundLeasesApi.getGroundLeases({ search: q, pageSize: 10 })
                    return (result.items ?? []).map(l => ({
                      id: l.id!,
                      label: [l.homeownerFullName, (l as { parcelAddress?: string }).parcelAddress]
                        .filter(Boolean).join(' — '),
                    }))
                  }}
                  placeholder="Search ground leases…"
                />
              </div>
              <div className="form-field full">
                <label className="form-label">Buyer Full Name</label>
                <input
                  className="form-input"
                  value={initiateForm.buyerFullName}
                  onChange={e => setInitiateForm(f => ({ ...f, buyerFullName: e.target.value }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Buyer Email</label>
                <input
                  className="form-input"
                  type="email"
                  value={initiateForm.buyerEmail}
                  onChange={e => setInitiateForm(f => ({ ...f, buyerEmail: e.target.value }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Buyer Phone (optional)</label>
                <input
                  className="form-input"
                  type="tel"
                  placeholder="+11231231234"
                  value={initiateForm.buyerPhone}
                  onChange={e => {
                    setInitiateError(null)
                    setInitiateForm(f => ({ ...f, buyerPhone: e.target.value }))
                  }}
                />
                <div className="form-help">Use E.164 format, for example +11231231234.</div>
              </div>
              <div className="form-field full">
                <label className="form-label">Purchase Price ($)</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  step="1"
                  value={initiateForm.purchasePrice}
                  onChange={e => setInitiateForm(f => ({ ...f, purchasePrice: parseFloat(e.target.value) || 0 }))}
                  required
                />
              </div>
            </div>
            {(initiateError || initiateMutation.isError) && (
              <div className="form-error">{initiateError ?? 'Failed to initiate workflow.'}</div>
            )}
          </form>
        </SlideOver>
      )}

    </div>
  )
}
