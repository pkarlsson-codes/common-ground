import { useState } from 'react'
import ForeclosurePreventionCaseCard from './ForeclosurePreventionCaseCard'
import { useForeclosurePreventionCases, useOpenForeclosurePreventionCase } from '../../hooks/stewardship'
import { useToast } from '../../hooks/useToast'

interface Props {
  groundLeaseId: string
}

const today = () => new Date().toISOString().slice(0, 10)

export default function ForeclosurePreventionSection({ groundLeaseId }: Props) {
  const { data: cases = [], isLoading } = useForeclosurePreventionCases(groundLeaseId)
  const openMutation = useOpenForeclosurePreventionCase(groundLeaseId)
  const { success, error: showError } = useToast()

  const [openFormVisible, setOpenFormVisible] = useState(false)
  const [openForm, setOpenForm] = useState({
    servicerName: '',
    delinquentMonths: 1,
    recordedDate: today(),
  })

  const handleOpen = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await openMutation.mutateAsync(openForm)
      success('Foreclosure prevention case opened')
      setOpenForm({ servicerName: '', delinquentMonths: 1, recordedDate: today() })
      setOpenFormVisible(false)
    } catch {
      showError('Failed to open case')
    }
  }

  return (
    <div className="detail-section">
      <div
        className="detail-section__title"
        style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}
      >
        <span>Foreclosure Prevention</span>
        <button
          type="button"
          className="btn btn-ghost btn-sm"
          onClick={() => setOpenFormVisible(!openFormVisible)}
        >
          {openFormVisible ? '×' : '+'}
        </button>
      </div>

      {openFormVisible && (
        <form onSubmit={handleOpen} style={{ marginBottom: 16 }}>
          <div className="form-grid" style={{ marginBottom: 12 }}>
            <div className="form-field">
              <label className="form-label">Servicer Name</label>
              <input
                className="form-input"
                value={openForm.servicerName}
                onChange={e => setOpenForm(f => ({ ...f, servicerName: e.target.value }))}
                required
              />
            </div>
            <div className="form-field">
              <label className="form-label">Delinquent Months</label>
              <input
                className="form-input"
                type="number"
                min="1"
                value={openForm.delinquentMonths}
                onChange={e => setOpenForm(f => ({ ...f, delinquentMonths: parseInt(e.target.value) || 1 }))}
                required
              />
            </div>
            <div className="form-field">
              <label className="form-label">Recorded Date</label>
              <input
                className="form-input"
                type="date"
                value={openForm.recordedDate}
                onChange={e => setOpenForm(f => ({ ...f, recordedDate: e.target.value }))}
                required
              />
            </div>
          </div>
          <button
            type="submit"
            className="btn btn-secondary"
            disabled={openMutation.isPending}
          >
            {openMutation.isPending ? 'Opening…' : 'Open Case'}
          </button>
          {openMutation.isError && <div className="form-error">Failed to open case.</div>}
        </form>
      )}

      {isLoading && <div style={{ color: 'var(--text-dim)', fontSize: 13 }}>Loading…</div>}

      {!isLoading && cases.length === 0 && !openFormVisible && (
        <div style={{ color: 'var(--text-dim)', fontSize: 13 }}>No foreclosure prevention cases.</div>
      )}

      {cases.map(c => (
        <ForeclosurePreventionCaseCard key={c.id} fpc={c} groundLeaseId={groundLeaseId} />
      ))}
    </div>
  )
}
