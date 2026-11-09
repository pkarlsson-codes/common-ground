import PageEyebrow from '../components/PageEyebrow'
import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { isOpenPurchasePhase, usePurchaseWorkflows, PURCHASE_STEP_LABELS } from '../hooks/purchaseWorkflows'
import { statusLabel, WORKFLOW_PHASES } from '../hooks/types'
import EntityCombobox from '../components/EntityCombobox'
import Pagination from '../components/Pagination'
import StatusBadge from '../components/StatusBadge'
import { GroundLeaseTemplatesApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import PurchaseWorkflowDetailPanel from './purchase-workflows/PurchaseWorkflowDetailPanel'
import PurchaseBoard from './purchase-workflows/PurchaseBoard'
import { readPreference, writePreference } from '../lib/preferences'
import { useToast } from '../hooks/useToast'

const groundLeaseTemplatesApi = new GroundLeaseTemplatesApi(apiConfig)

type Panel = 'detail' | null
type View = 'board' | 'list'

const VIEW_KEY = 'cg.purchases.view'
const readView = (): View => (readPreference(VIEW_KEY) === 'list' ? 'list' : 'board')

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

export default function PurchaseWorkflowsPage() {
  const location = useLocation()
  const navigate = useNavigate()

  const [page, setPage] = useState(1)
  const [phaseFilter, setPhaseFilter] = useState('')
  const [templateFilter, setTemplateFilter] = useState('')
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [panel, setPanel] = useState<Panel>(null)
  const [view, setViewState] = useState<View>(readView)

  const setView = (value: View) => {
    setViewState(value)
    writePreference(VIEW_KEY, value)
  }

  const { success, error } = useToast()

  const { data, isLoading, isError, refetch } = usePurchaseWorkflows(page, templateFilter || undefined, phaseFilter || undefined, undefined, view === 'list')

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

  const clearTemplateFilter = () => {
    setTemplateFilter('')
    setPage(1)
  }

  const workflows = data?.items ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <PageEyebrow />
          <h1 className="page-header__title">Purchases</h1>
          {view === 'list' && data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} workflow{data.totalCount !== 1 ? 's' : ''}
            </div>
          )}
        </div>
        <div className="today-toggle" role="group" aria-label="View">
          <button type="button" className="btn btn-secondary btn-sm" aria-pressed={view === 'board'} onClick={() => setView('board')}>Board</button>
          <button type="button" className="btn btn-secondary btn-sm" aria-pressed={view === 'list'} onClick={() => setView('list')}>List</button>
        </div>
      </div>

      {view === 'board' && <PurchaseBoard selectedId={panel === 'detail' ? selectedId : null} onOpen={openDetail} />}

      {view === 'list' && (<>

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
            value={templateFilter || null}
            onChange={id => { setTemplateFilter(id ?? ''); setPage(1) }}
            fetchOptions={async q => {
              const result = await groundLeaseTemplatesApi.getGroundLeaseTemplates({ search: q, pageSize: 10 })
              return (result.items ?? []).map(t => ({
                id: t.id!,
                label: t.templateName ?? t.id!,
              }))
            }}
            placeholder="Filter by lease template…"
          />
          {templateFilter && (
            <button type="button" className="btn btn-ghost btn-sm" onClick={clearTemplateFilter}>
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
            <div className="error-state__sub">Try again in a moment. If it keeps failing, contact support.</div>
            <div style={{ marginTop: 10 }}>
              <button className="btn btn-ghost btn-sm" type="button" onClick={() => { void refetch() }}>Retry</button>
            </div>
          </div>
        )}

        {!isLoading && !isError && workflows.length === 0 && (
          <div className="empty-state">
            <div className="empty-state__glyph">⊡</div>
            <div className="empty-state__title">No workflows found</div>
            <div className="empty-state__sub">Workflows start from selecting an applicant on a home.</div>
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
                  <th>Waiting On</th>
                  <th>Lease Template</th>
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
                    <td>{isOpenPurchasePhase(w.phase) ? PURCHASE_STEP_LABELS[w.nextStep ?? ''] ?? w.nextStep : '—'}</td>
                    <td>
                      <span className="cell-id" title={w.groundLeaseTemplateId}>
                        {w.groundLeaseTemplateId?.slice(0, 8)}…
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

      </>)}

      {/* ── Detail panel ─── */}
      {panel === 'detail' && (
        <PurchaseWorkflowDetailPanel
          selectedId={selectedId!}
          onClose={closePanel}
          showToast={showToast}
        />
      )}

    </div>
  )
}
