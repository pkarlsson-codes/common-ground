import { useState } from 'react'
import type { InsuranceCertificateDto } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useRemoveInsuranceCertificate } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { formatCalendarDate } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  groundLeaseId: string
  certificate: InsuranceCertificateDto
  onClose: () => void
}

/** Removes a certificate recorded in error. It stays on the lease, greyed, with the reason; its file stays in Documents. */
export default function RemoveInsuranceCertificatePanel({ groundLeaseId, certificate, onClose }: Props) {
  const [reason, setReason] = useState('')
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useRemoveInsuranceCertificate()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!reason.trim()) return
    setProblem(null)
    try {
      await mutation.mutateAsync({ groundLeaseId, certificateId: certificate.certificateId!, reason: reason.trim() })
      success('Insurance certificate removed')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to remove the insurance certificate.'))
    }
  }

  const period = `${certificate.policyStart ? formatCalendarDate(certificate.policyStart) : '—'} – ${certificate.policyEnd ? formatCalendarDate(certificate.policyEnd) : '—'}`

  return (
    <SlideOver
      title="Remove Insurance Certificate"
      subtitle={`${certificate.insurer} · ${certificate.policyNumber} · ${period}`}
      onClose={onClose}
      isDirty={!!reason}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="remove-insurance-certificate-form"
            disabled={mutation.isPending || !reason.trim()}
          >
            {mutation.isPending ? 'Removing…' : 'Remove Certificate'}
          </button>
        </>
      }
    >
      <form id="remove-insurance-certificate-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          For a certificate recorded in error. It stays on the lease, greyed, with your reason, and no longer counts toward
          covered-through or the check. Its file stays in Documents.
        </p>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label" htmlFor="certificate-remove-reason">Reason</label>
            <textarea
              id="certificate-remove-reason"
              className="form-textarea"
              value={reason}
              onChange={e => setReason(e.target.value)}
              placeholder="Policy end typed as 2028, should be 2027"
              required
            />
          </div>
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
