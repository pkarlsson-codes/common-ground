import { useState } from 'react'
import SlideOver from '../SlideOver'
import EntityCombobox from '../EntityCombobox'
import { useOpenFpcCase } from '../../hooks/stewardship'
import { useToast } from '../../hooks/useToast'
import { GroundLeasesApi } from '../../api'
import { apiConfig } from '../../lib/apiConfig'

const groundLeasesApi = new GroundLeasesApi(apiConfig)

interface OpenFpcForm {
  groundLeaseId: string
  servicerName: string
  delinquentMonths: number | ''
  recordedDate: string
}

interface Props {
  onClose: () => void
}

export default function FpcOpenCasePanel({ onClose }: Props) {
  const { success } = useToast()
  const openCase = useOpenFpcCase()

  const [form, setForm] = useState<OpenFpcForm>({
    groundLeaseId: '',
    servicerName: '',
    delinquentMonths: '',
    recordedDate: new Date().toISOString().slice(0, 10),
  })

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!form.groundLeaseId || !form.servicerName || !form.delinquentMonths || !form.recordedDate) return
    try {
      await openCase.mutateAsync({
        groundLeaseId: form.groundLeaseId,
        servicerName: form.servicerName,
        delinquentMonths: Number(form.delinquentMonths),
        recordedDate: form.recordedDate,
      })
      success('Foreclosure prevention case opened')
      onClose()
    } catch {
      // error shown inline via openCase.isError
    }
  }

  return (
    <SlideOver
      title="Open Foreclosure Prevention Case"
      subtitle="Record a servicer delinquency and open a new case"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="fpc-open-case-form"
            disabled={
              openCase.isPending ||
              !form.groundLeaseId ||
              !form.servicerName ||
              !form.delinquentMonths
            }
          >
            {openCase.isPending ? 'Opening…' : 'Open Case'}
          </button>
        </>
      }
    >
      <form id="fpc-open-case-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Ground Lease</label>
            <EntityCombobox
              value={form.groundLeaseId || null}
              onChange={id => setForm(f => ({ ...f, groundLeaseId: id ?? '' }))}
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
          <div className="form-field">
            <label className="form-label">Servicer Name</label>
            <input
              className="form-input"
              value={form.servicerName}
              onChange={e => setForm(f => ({ ...f, servicerName: e.target.value }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Delinquent Months</label>
            <input
              className="form-input"
              type="number"
              min="1"
              max="120"
              value={form.delinquentMonths}
              onChange={e =>
                setForm(f => ({
                  ...f,
                  delinquentMonths: e.target.value === '' ? '' : parseInt(e.target.value),
                }))
              }
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Recorded Date</label>
            <input
              className="form-input"
              type="date"
              value={form.recordedDate}
              onChange={e => setForm(f => ({ ...f, recordedDate: e.target.value }))}
              required
            />
          </div>
        </div>
        {openCase.isError && (
          <div className="form-error">Failed to open case. Check inputs and try again.</div>
        )}
      </form>
    </SlideOver>
  )
}
