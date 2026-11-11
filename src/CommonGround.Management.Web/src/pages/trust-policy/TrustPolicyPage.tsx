import { useState } from 'react'
import type { LeaseFeeNoticePolicyResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import PageEyebrow from '../../components/PageEyebrow'
import { useLeaseFeeNoticePolicy, useSetLeaseFeeNoticePolicy, type LeaseFeeNoticePolicyForm as PolicyForm } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

/** The trust's own policies. For now: when late lease fee notices fall due, and how the letters are signed. */
export default function TrustPolicyPage() {
  const { data, isLoading, isError } = useLeaseFeeNoticePolicy()

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <PageEyebrow />
          <h1 className="page-header__title">Trust Policy</h1>
        </div>
      </div>

      {isLoading && <div className="skeleton-rows"><div className="skeleton-row" /></div>}
      {isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load the policy</div>
          <div className="error-state__sub">Try again in a moment. If it keeps failing, contact support.</div>
        </div>
      )}
      {data && <LeaseFeeNoticePolicyForm policy={data} />}
    </div>
  )
}

function LeaseFeeNoticePolicyForm({ policy }: { policy: LeaseFeeNoticePolicyResponse }) {
  const mutation = useSetLeaseFeeNoticePolicy()
  const { success } = useToast()
  const [form, setForm] = useState<PolicyForm>(() => ({
    firstNoticeDays: policy.firstNoticeDays ?? 10,
    secondNoticeDays: policy.secondNoticeDays ?? 30,
    letterSignOff: policy.letterSignOff ?? '',
  }))
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)

  const valid = form.firstNoticeDays >= 1 && form.secondNoticeDays > form.firstNoticeDays

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      await mutation.mutateAsync(form)
      success('Policy saved')
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to save the policy.'))
    }
  }

  return (
    <form onSubmit={handleSubmit} style={{ maxWidth: 560 }}>
      <div className="detail-section">
        <div className="detail-section__title">Lease Fee Notices</div>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 12 }}>
          When a lease fee is this many days overdue, the lease shows on Today and on the Lease Fees tab as needing a
          notice. Staff print the letter, send it, and record it.
          {policy.isDefault && ' Using the default policy.'}
        </p>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="policy-first-days">First Notice After (days overdue)</label>
            <input
              id="policy-first-days"
              className="form-input"
              type="number"
              min="1"
              step="1"
              value={form.firstNoticeDays || ''}
              onChange={e => setForm(f => ({ ...f, firstNoticeDays: parseInt(e.target.value, 10) || 0 }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="policy-second-days">Second Notice After (days overdue)</label>
            <input
              id="policy-second-days"
              className="form-input"
              type="number"
              min={form.firstNoticeDays + 1}
              step="1"
              value={form.secondNoticeDays || ''}
              onChange={e => setForm(f => ({ ...f, secondNoticeDays: parseInt(e.target.value, 10) || 0 }))}
              required
            />
            {form.secondNoticeDays <= form.firstNoticeDays && (
              <div className="form-help" style={{ color: 'var(--red)' }}>Must be more than First Notice After.</div>
            )}
          </div>
          <div className="form-field" style={{ gridColumn: '1 / -1' }}>
            <label className="form-label" htmlFor="policy-sign-off">Letter Sign-Off</label>
            <textarea
              id="policy-sign-off"
              className="form-textarea"
              rows={4}
              value={form.letterSignOff}
              onChange={e => setForm(f => ({ ...f, letterSignOff: e.target.value }))}
              placeholder={'Your trust\'s name\nAddress\nPhone'}
            />
            <div className="form-help">Printed under "Sincerely," at the end of every notice letter.</div>
          </div>
        </div>
      </div>

      <ApiProblemAlert problem={problem} />

      <button type="submit" className="btn btn-primary" disabled={!valid || mutation.isPending}>
        {mutation.isPending ? 'Saving…' : 'Save Policy'}
      </button>
    </form>
  )
}
