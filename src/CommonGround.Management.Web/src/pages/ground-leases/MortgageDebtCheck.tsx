import type { MortgageRequestResponse } from '../../api'
import { formatPrice } from '../../lib/currency'
import { debtCheckText } from './mortgageRequestLetter'

/** The request's total debt against the lease's cap on the purchase option price (Art 8.3). Null without the appraisal. */
export default function MortgageDebtCheck({ request }: { request: MortgageRequestResponse }) {
  const check = debtCheckText(request)
  if (!check) return null
  return (
    <>
      <div className="detail-grid">
        <div className="detail-field">
          <div className="detail-field__label">Purchase Option Price</div>
          <div className="detail-field__value cell-mono">{formatPrice(request.purchaseOptionPrice ?? 0)}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Debt Cap</div>
          <div className="detail-field__value">
            {request.maxMortgageDebtPercent}% of the purchase option price: <span className="cell-mono">{formatPrice(request.debtCapAmount ?? 0)}</span>
          </div>
        </div>
      </div>
      <p style={{ fontSize: 13, marginTop: 8 }}>
        {check.share}{' '}
        <strong style={{ color: check.overCap ? 'var(--error-bright)' : 'var(--success)' }}>{check.verdict}</strong>
      </p>
    </>
  )
}
