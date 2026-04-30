import { useState } from 'react'
import {
  useDevelopmentProject,
  useActivateDevelopmentProject,
  useCompleteDevelopmentProject,
  useAbandonDevelopmentProject,
} from '../../hooks/developmentProjects'
import SlideOver from '../../components/SlideOver'
import StatusBadge from '../../components/StatusBadge'
import NavIdLink from '../../components/NavIdLink'
import { useToast } from '../../hooks/useToast'

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

interface Props {
  projectId: string
  onClose: () => void
}

export default function DevelopmentProjectDetailPanel({ projectId, onClose }: Props) {
  const { data: detail, isLoading } = useDevelopmentProject(projectId)
  const activate = useActivateDevelopmentProject()
  const complete = useCompleteDevelopmentProject()
  const abandon = useAbandonDevelopmentProject()
  const [completionDate, setCompletionDate] = useState(new Date().toISOString().slice(0, 10))
  const [showCompleteForm, setShowCompleteForm] = useState(false)
  const [menuOpen, setMenuOpen] = useState(false)
  const [confirmAbandon, setConfirmAbandon] = useState(false)
  const { success } = useToast()

  const isPending = activate.isPending || complete.isPending || abandon.isPending

  const handleActivate = async () => {
    try {
      await activate.mutateAsync(projectId)
      success('Project activated')
    } catch { /* error via mutation state */ }
  }

  const handleComplete = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await complete.mutateAsync({ id: projectId, actualCompletionDate: completionDate })
      success('Project marked complete')
      setShowCompleteForm(false)
    } catch { /* error via mutation state */ }
  }

  const handleAbandon = async () => {
    try {
      await abandon.mutateAsync(projectId)
      success('Project abandoned')
      setConfirmAbandon(false)
    } catch { /* error via mutation state */ }
  }

  return (
    <SlideOver
      title={detail?.name ?? '…'}
      subtitle={detail ? `${detail.status}` : undefined}
      onClose={onClose}
    >
      {isLoading || !detail ? <SkeletonRows /> : (
        <>
          <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: 8, position: 'relative' }}>
            {detail.status !== 'Completed' && detail.status !== 'Abandoned' && (
              <>
                <button
                  type="button"
                  className="btn btn-ghost btn-sm"
                  onClick={() => setMenuOpen(o => !o)}
                  aria-label="Actions"
                >
                  ···
                </button>
                {menuOpen && (
                  <div
                    style={{
                      position: 'absolute',
                      top: '100%',
                      right: 0,
                      zIndex: 10,
                      background: 'var(--surface-1)',
                      border: '1px solid var(--border)',
                      borderRadius: 8,
                      padding: '4px 0',
                      minWidth: 160,
                      boxShadow: '0 4px 16px rgba(0,0,0,0.3)',
                    }}
                  >
                    <button
                      type="button"
                      className="btn btn-ghost"
                      style={{ width: '100%', textAlign: 'left', borderRadius: 0, padding: '8px 16px' }}
                      onClick={() => { setMenuOpen(false); setConfirmAbandon(true) }}
                    >
                      Abandon Project
                    </button>
                  </div>
                )}
              </>
            )}
          </div>

          {confirmAbandon && (
            <div
              style={{
                border: '1px solid var(--border)',
                borderRadius: 8,
                padding: 12,
                marginBottom: 16,
                background: 'var(--surface-1)',
              }}
            >
              <div style={{ fontSize: 13, marginBottom: 10 }}>
                Abandon this project? This cannot be undone.
              </div>
              <div style={{ display: 'flex', gap: 8 }}>
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={handleAbandon}
                  disabled={isPending}
                >
                  {abandon.isPending ? 'Saving…' : 'Confirm'}
                </button>
                <button
                  type="button"
                  className="btn btn-ghost"
                  onClick={() => setConfirmAbandon(false)}
                  disabled={isPending}
                >
                  Cancel
                </button>
              </div>
              {abandon.isError && (
                <div className="form-error" style={{ marginTop: 8 }}>Action failed. Please try again.</div>
              )}
            </div>
          )}

          {detail.status === 'Initiated' && (
            <div className="detail-section">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={handleActivate}
                disabled={isPending}
              >
                {activate.isPending ? 'Activating…' : 'Activate Project'}
              </button>
              {activate.isError && <div className="form-error">Action failed.</div>}
            </div>
          )}

          {detail.status === 'Active' && !showCompleteForm && (
            <div className="detail-section">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => setShowCompleteForm(true)}
                disabled={isPending}
              >
                Mark Complete
              </button>
            </div>
          )}

          {detail.status === 'Active' && showCompleteForm && (
            <div className="detail-section" style={{ border: '1px solid var(--border)', borderRadius: 8, padding: 12, marginBottom: 16 }}>
              <form onSubmit={handleComplete}>
                <div className="form-field" style={{ marginBottom: 10 }}>
                  <label className="form-label">Actual Completion Date</label>
                  <input
                    className="form-input"
                    type="date"
                    value={completionDate}
                    onChange={e => setCompletionDate(e.target.value)}
                    required
                  />
                </div>
                <div style={{ display: 'flex', gap: 8 }}>
                  <button
                    type="submit"
                    className="btn btn-secondary"
                    disabled={isPending}
                  >
                    {complete.isPending ? 'Saving…' : 'Confirm Completion'}
                  </button>
                  <button
                    type="button"
                    className="btn btn-ghost"
                    onClick={() => setShowCompleteForm(false)}
                  >
                    Cancel
                  </button>
                </div>
                {complete.isError && <div className="form-error">Action failed.</div>}
              </form>
            </div>
          )}

          <div className="detail-grid">
            <div className="detail-field">
              <div className="detail-field__label">Status</div>
              <div className="detail-field__value"><StatusBadge status={detail.status} /></div>
            </div>
            {detail.projectedStartDate && (
              <div className="detail-field">
                <div className="detail-field__label">Projected Start</div>
                <div className="detail-field__value mono">
                  {new Date(detail.projectedStartDate).toLocaleDateString()}
                </div>
              </div>
            )}
            {detail.projectedEndDate && (
              <div className="detail-field">
                <div className="detail-field__label">Projected End</div>
                <div className="detail-field__value mono">
                  {new Date(detail.projectedEndDate).toLocaleDateString()}
                </div>
              </div>
            )}
            {detail.actualCompletionDate && (
              <div className="detail-field">
                <div className="detail-field__label">Completed</div>
                <div className="detail-field__value mono">
                  {new Date(detail.actualCompletionDate).toLocaleDateString()}
                </div>
              </div>
            )}
            {detail.budget != null && (
              <div className="detail-field">
                <div className="detail-field__label">Budget</div>
                <div className="detail-field__value mono">
                  {new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(detail.budget)}
                </div>
              </div>
            )}
            {detail.description && (
              <div className="detail-field full-span">
                <div className="detail-field__label">Description</div>
                <div className="detail-field__value">{detail.description}</div>
              </div>
            )}
            <div className="detail-field full-span">
              <div className="detail-field__value">
                <NavIdLink id={detail.landParcelId} to="/land-parcels" label="Open Land Parcel" />
              </div>
            </div>
          </div>
        </>
      )}
    </SlideOver>
  )
}
