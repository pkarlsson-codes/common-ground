import { useEffect, useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import {
  NOTICE_DELIVERIES,
  NOTICE_DELIVERY_LABELS,
  useRecordResalePriceStatement,
  useResalePriceStatementPreview,
  type NoticeDelivery,
  type ResalePriceStatementFigures,
} from '../../hooks/groundLeases'
import { useHome } from '../../hooks/homes'
import { useSettled } from '../../hooks/useSettled'
import { useToast } from '../../hooks/useToast'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import {
  cannotPriceReason,
  emptyStatementFigures,
  formulaText,
  resalePriceStatementLetterPath,
  statementFiguresComplete,
  statementWorking,
} from './resalePriceStatementLetter'

interface Props {
  lease: GroundLeaseResponse
  onClose: () => void
}

const moneyOrNull = (value: string) => (value === '' ? null : Math.max(0, parseFloat(value) || 0))

/**
 * Works out one lease's resale price on a statement date (Ch 23, Disclosure), prints the letter, and records it sent.
 * Asks only for the figures the lease's formula needs.
 */
export default function PrepareResalePriceStatementPanel({ lease, onClose }: Props) {
  const today = trustToday()
  // Confirmed improvement credits on the lease are the starting figure; the cap applies in the price calculation.
  const initialFigures = () => ({
    ...emptyStatementFigures(),
    improvementsCredit: lease.improvementsCap != null && lease.approvedImprovements ? lease.approvedImprovements : null,
  })
  const [statementDate, setStatementDate] = useState<CalendarDate | null>(today)
  const [figures, setFigures] = useState<ResalePriceStatementFigures>(initialFigures)
  const [delivery, setDelivery] = useState<NoticeDelivery>('FirstClassMail')
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  // The date and figures the letter was last printed with, so a change after printing is pointed out before recording.
  const [printedKey, setPrintedKey] = useState<string | null>(null)
  const { data: home } = useHome(lease.homeId ?? null)
  const mutation = useRecordResalePriceStatement()
  const { success } = useToast()

  const reason = cannotPriceReason(lease)
  const dateValid = !!statementDate && statementDate <= today && statementDate >= (lease.leaseStartDate ?? statementDate)
  const complete = dateValid && statementFiguresComplete(lease, figures)
  const key = JSON.stringify([statementDate, figures])
  const settledKey = useSettled(key)
  const current = complete && settledKey === key
  const preview = useResalePriceStatementPreview(lease.id ?? null, statementDate, figures, current)
  const price = current ? preview.data ?? null : null

  // The preview's refusal, such as a statement date before the lease start; shown while that preview is the current one.
  const [previewProblem, setPreviewProblem] = useState<ApiProblemDetails | null>(null)
  useEffect(() => {
    if (!preview.error) return
    let cancelled = false
    getApiProblem(preview.error, "Couldn't work out the price.").then(p => { if (!cancelled) setPreviewProblem(p) })
    return () => { cancelled = true }
  }, [preview.error])

  const setFigure = (change: Partial<ResalePriceStatementFigures>) => setFigures(f => ({ ...f, ...change }))

  const handlePrint = () => {
    if (!statementDate) return
    setPrintedKey(key)
    window.open(resalePriceStatementLetterPath(lease.id!, statementDate, figures), '_blank')
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!statementDate || !price) return
    setProblem(null)
    try {
      await mutation.mutateAsync({ groundLeaseId: lease.id!, statementDate, figures, delivery })
      success('Statement recorded')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record the statement.'))
    }
  }

  const empty = initialFigures()
  const isDirty = statementDate !== today
    || figures.marketValue !== empty.marketValue
    || figures.marketValueSource !== empty.marketValueSource
    || figures.indexValue !== empty.indexValue
    || figures.improvementsCredit !== empty.improvementsCredit

  return (
    <SlideOver
      title="Resale Price Statement"
      subtitle={lease.homeownerFullName ?? 'Ground lease'}
      onClose={onClose}
      isDirty={isDirty}
      footer={reason ? <SlideOverCancelButton /> : (
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="prepare-resale-price-statement-form"
            disabled={mutation.isPending || !price}
          >
            {mutation.isPending ? 'Recording…' : 'Record Sent'}
          </button>
        </>
      )}
    >
      <div className="detail-grid" style={{ marginBottom: 16 }}>
        <div className="detail-field">
          <div className="detail-field__label">Homeowner</div>
          <div className="detail-field__value">{lease.homeownerFullName ?? '—'}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Home</div>
          <div className="detail-field__value">{home?.label ?? '—'}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Formula</div>
          <div className="detail-field__value">{formulaText(lease)}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Base Price</div>
          <div className="detail-field__value mono">{formatPrice(lease.basePrice ?? 0)}</div>
        </div>
      </div>

      {reason ? (
        <p className="form-error" role="alert">{reason}</p>
      ) : (
        <form id="prepare-resale-price-statement-form" onSubmit={handleSubmit}>
          <div className="form-grid">
            <div className="form-field">
              <label className="form-label" htmlFor="statement-date">Statement Date</label>
              <DateInput
                id="statement-date"
                className="form-input"
                value={statementDate}
                min={lease.leaseStartDate}
                max={today}
                onChange={setStatementDate}
              />
              <div className="form-help">The letter's date, the day the price is worked out on, and the day it's sent.</div>
            </div>

            {lease.formulaKind === 'AppraisalBased' && (
              <>
                <div className="form-field">
                  <label className="form-label" htmlFor="statement-market-value">Market Value ($)</label>
                  <input
                    id="statement-market-value"
                    className="form-input"
                    type="number"
                    min="0.01"
                    step="0.01"
                    value={figures.marketValue ?? ''}
                    onChange={e => setFigure({ marketValue: moneyOrNull(e.target.value) })}
                    required
                  />
                  <div className="form-help">An estimate of what the home would sell for today, not an appraisal.</div>
                </div>
                <div className="form-field full">
                  <label className="form-label" htmlFor="statement-value-source">Value Source</label>
                  <input
                    id="statement-value-source"
                    className="form-input"
                    value={figures.marketValueSource}
                    onChange={e => setFigure({ marketValueSource: e.target.value })}
                    placeholder="County assessment 2026"
                    required
                  />
                  <div className="form-help">Printed on the letter next to the market value.</div>
                </div>
              </>
            )}

            {lease.formulaKind === 'IndexBased' && (
              <div className="form-field">
                <label className="form-label" htmlFor="statement-index-value">
                  {lease.indexType ?? 'Index'} value on the statement date
                </label>
                <input
                  id="statement-index-value"
                  className="form-input"
                  type="number"
                  min="0.0001"
                  step="any"
                  value={figures.indexValue ?? ''}
                  onChange={e => setFigure({ indexValue: e.target.value === '' ? null : Math.max(0, parseFloat(e.target.value) || 0) })}
                  required
                />
                <div className="form-help">At purchase: {lease.baselineIndexValue}</div>
              </div>
            )}

            {lease.improvementsCap != null && (
              <div className="form-field">
                <label className="form-label" htmlFor="statement-improvements-credit">Improvements Credit ($)</label>
                <input
                  id="statement-improvements-credit"
                  className="form-input"
                  type="number"
                  min="0"
                  step="0.01"
                  value={figures.improvementsCredit ?? ''}
                  onChange={e => setFigure({ improvementsCredit: moneyOrNull(e.target.value) })}
                />
                <div className="form-help">Approved improvements, up to {formatPrice(lease.improvementsCap)}</div>
              </div>
            )}
          </div>

          <div className="detail-section" aria-live="polite">
            <div className="detail-section__title">Maximum Resale Price</div>
            {price ? (
              <>
                <div className="mono" style={{ fontSize: 20, marginBottom: 8 }}>{formatPrice(price.price ?? 0)}</div>
                {statementWorking(lease, price).map(line => (
                  <div key={line} style={{ fontSize: 13, color: 'var(--text-secondary)' }}>{line}</div>
                ))}
              </>
            ) : (
              <p style={{ fontSize: 13, color: 'var(--text-dim)' }}>
                {complete && !preview.isError ? 'Working out the price…' : 'Enter the figures above to see the price.'}
              </p>
            )}
          </div>

          <div className="form-grid">
            <div className="form-field">
              <button type="button" className="btn btn-secondary" onClick={handlePrint} disabled={!price}>
                Print Letter
              </button>
            </div>
            <div className="form-field">
              <label className="form-label" htmlFor="statement-delivery">Delivery</label>
              <select
                id="statement-delivery"
                className="form-select"
                value={delivery}
                onChange={e => setDelivery(e.target.value as NoticeDelivery)}
              >
                {NOTICE_DELIVERIES.map(d => <option key={d} value={d}>{NOTICE_DELIVERY_LABELS[d]}</option>)}
              </select>
              <div className="form-help">Record it once the letter is posted, emailed or handed over.</div>
              {printedKey && printedKey !== key && (
                <div className="form-help" style={{ color: 'var(--red)' }}>
                  The date or figures changed after the letter was printed. Print it again so the letter matches what is recorded.
                </div>
              )}
            </div>
          </div>

          <ApiProblemAlert problem={problem ?? (current && preview.error ? previewProblem : null)} />
        </form>
      )}
    </SlideOver>
  )
}
