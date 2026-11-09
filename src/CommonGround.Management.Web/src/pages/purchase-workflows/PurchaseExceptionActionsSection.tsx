import { useState, type FormEvent } from 'react'
import type { FailWorkflowForm, WithdrawWorkflowForm } from '../../hooks/types'
import ConfirmActionModal from '../../components/ConfirmActionModal'

type ConfirmKind = 'fail' | 'withdraw' | null

interface PurchaseExceptionActionsSectionProps {
  canFail: boolean
  canWithdraw: boolean
  failForm: FailWorkflowForm
  setFailForm: (updater: (f: FailWorkflowForm) => FailWorkflowForm) => void
  withdrawForm: WithdrawWorkflowForm
  setWithdrawForm: (updater: (f: WithdrawWorkflowForm) => WithdrawWorkflowForm) => void
  failPending: boolean
  withdrawPending: boolean
  onFail: (e: FormEvent, form: FailWorkflowForm) => void | Promise<void>
  onWithdraw: (e: FormEvent, form: WithdrawWorkflowForm) => void | Promise<void>
}

export default function PurchaseExceptionActionsSection({
  canFail,
  canWithdraw,
  failForm,
  setFailForm,
  withdrawForm,
  setWithdrawForm,
  failPending,
  withdrawPending,
  onFail,
  onWithdraw,
}: PurchaseExceptionActionsSectionProps) {
  const [pendingConfirm, setPendingConfirm] = useState<ConfirmKind>(null)

  if (!canFail && !canWithdraw) return null

  const requestFail = (e: FormEvent) => {
    e.preventDefault()
    if (!failForm.reason.trim()) return
    setPendingConfirm('fail')
  }

  const requestWithdraw = (e: FormEvent) => {
    e.preventDefault()
    if (!withdrawForm.reason.trim()) return
    setPendingConfirm('withdraw')
  }

  const handleConfirm = () => {
    const kind = pendingConfirm
    setPendingConfirm(null)
    if (kind === 'fail') {
      const synthetic = { preventDefault: () => {} } as unknown as FormEvent
      void onFail(synthetic, failForm)
    } else if (kind === 'withdraw') {
      const synthetic = { preventDefault: () => {} } as unknown as FormEvent
      void onWithdraw(synthetic, withdrawForm)
    }
  }

  return (
    <div className="detail-section">
      <div className="detail-section__title">Exception Actions</div>

      {canFail && (
        <form onSubmit={requestFail} style={{ marginBottom: 16 }}>
          <div className="form-grid" style={{ marginBottom: 8 }}>
            <div className="form-field full">
              <label className="form-label">Fail Reason</label>
              <input
                className="form-input"
                value={failForm.reason}
                onChange={e => setFailForm(f => ({ ...f, reason: e.target.value }))}
                placeholder="Why is this purchase failing?"
                required
              />
            </div>
          </div>
          <button
            type="submit"
            className="btn btn-danger-ghost"
            disabled={failPending}
          >
            {failPending ? 'Failing…' : 'Mark as Failed'}
          </button>
        </form>
      )}

      {canWithdraw && (
        <form onSubmit={requestWithdraw}>
          <div className="form-grid" style={{ marginBottom: 8 }}>
            <div className="form-field full">
              <label className="form-label">Withdrawal Reason</label>
              <input
                className="form-input"
                value={withdrawForm.reason}
                onChange={e => setWithdrawForm(f => ({ ...f, reason: e.target.value }))}
                placeholder="Why is the buyer withdrawing?"
                required
              />
            </div>
          </div>
          <button
            type="submit"
            className="btn btn-danger-ghost"
            disabled={withdrawPending}
          >
            {withdrawPending ? 'Withdrawing…' : 'Withdraw Purchase'}
          </button>
        </form>
      )}

      <ConfirmActionModal
        open={pendingConfirm === 'fail'}
        title="Mark as Failed"
        message="Mark this workflow as failed? This action cannot be undone."
        confirmLabel="Mark as Failed"
        confirmTone="danger"
        onConfirm={handleConfirm}
        onCancel={() => setPendingConfirm(null)}
      />
      <ConfirmActionModal
        open={pendingConfirm === 'withdraw'}
        title="Withdraw Purchase"
        message="Withdraw this purchase workflow? This action cannot be undone."
        confirmLabel="Withdraw Purchase"
        confirmTone="danger"
        onConfirm={handleConfirm}
        onCancel={() => setPendingConfirm(null)}
      />
    </div>
  )
}
