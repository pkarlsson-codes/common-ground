import { useState } from 'react'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import {
  NOTICE_DELIVERIES,
  NOTICE_DELIVERY_LABELS,
  useRecordResalePriceStatement,
  useResalePriceStatementPreviews,
  useResalePriceStatementsDue,
  type NoticeDelivery,
  type RoundLeaseFigures,
} from '../../hooks/groundLeases'
import { useSettled } from '../../hooks/useSettled'
import { useToast } from '../../hooks/useToast'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem } from '../../lib/getErrorMessage'
import { cannotPriceReason } from '../ground-leases/resalePriceStatementLetter'
import {
  filledSources,
  marketValueLeases,
  printedRoundPath,
  roundFigures,
  roundIndexTypes,
  savePrintedRound,
  type MarketValueEntry,
  type RoundEntries,
} from './resalePriceRound'

interface Props {
  onClose: () => void
}

interface Failure {
  homeowner: string
  reason: string
}

const numberOrNull = (value: string) => (value === '' ? null : Math.max(0, parseFloat(value) || 0))
const emptyEntries = (): RoundEntries => ({ indexValues: {}, marketValues: {} })

/**
 * The yearly round of resale price statements (Ch 23, Disclosure): the figures entered once, every price worked out,
 * all letters printed, and the ticked ones recorded sent with one date and delivery. A lease that can't be priced is
 * listed with why and left out. A lease that fails to record is listed with why; the others are still recorded.
 */
export default function PrepareResalePriceStatementsPanel({ onClose }: Props) {
  const today = trustToday()
  const { data, isLoading } = useResalePriceStatementsDue(false, '')
  const leases = data?.leases ?? []
  const [statementDate, setStatementDate] = useState<CalendarDate | null>(today)
  const [entries, setEntries] = useState<RoundEntries>(emptyEntries)
  // Every lease that can be priced starts ticked; one that comes due while the panel is open starts unticked.
  const [tickedIds, setTickedIds] = useState<Set<string> | null>(null)
  const [delivery, setDelivery] = useState<NoticeDelivery>('FirstClassMail')
  const [failures, setFailures] = useState<Failure[]>([])
  const [pending, setPending] = useState(false)
  const [printProblem, setPrintProblem] = useState<string | null>(null)
  // The date and figures the letters were last printed with, so a change after printing is pointed out before recording.
  const [printedKey, setPrintedKey] = useState<string | null>(null)
  const mutation = useRecordResalePriceStatement()
  const { success } = useToast()

  const indexTypes = roundIndexTypes(leases)
  const marketRows = marketValueLeases(leases)
  const sources = filledSources(marketRows, entries.marketValues)
  const figures = roundFigures(leases, entries)
  const dateValid = !!statementDate && statementDate <= today

  const key = JSON.stringify([statementDate, figures])
  const current = useSettled(key) === key
  const preview = useResalePriceStatementPreviews(statementDate, figures, dateValid && current)
  const results = new Map((current && preview.data?.leases || []).map(r => [r.groundLeaseId!, r]))

  const priceable = leases.filter(l => !cannotPriceReason(l))
  if (data && tickedIds === null) setTickedIds(new Set(priceable.map(l => l.groundLeaseId!)))
  const ticked = priceable.filter(l => tickedIds?.has(l.groundLeaseId!))
  const tickKey = ticked.map(l => l.groundLeaseId).sort()
  const tickedReady = ticked.filter(l => results.get(l.groundLeaseId!)?.price)
  const allReady = ticked.length > 0 && tickedReady.length === ticked.length
  const tickedFigures = (): RoundLeaseFigures[] => figures.filter(f => ticked.some(l => l.groundLeaseId === f.groundLeaseId))

  const toggle = (id: string) => setTickedIds(prev => {
    const next = new Set(prev)
    if (next.has(id)) next.delete(id)
    else next.add(id)
    return next
  })

  const toggleAll = () =>
    setTickedIds(ticked.length === priceable.length ? new Set() : new Set(priceable.map(l => l.groundLeaseId!)))

  const setIndexValue = (indexType: string, value: number | null) =>
    setEntries(e => ({ ...e, indexValues: { ...e.indexValues, [indexType]: value } }))
  const setMarketValue = (id: string, change: Partial<MarketValueEntry>) =>
    setEntries(e => ({
      ...e,
      marketValues: { ...e.marketValues, [id]: { ...(e.marketValues[id] ?? { value: null, source: '' }), ...change } },
    }))

  const handlePrint = () => {
    if (!statementDate) return
    const id = savePrintedRound({ statementDate, leases: tickedFigures() })
    if (!id) {
      setPrintProblem("This browser won't keep the figures for the print page. Allow site data for this app and try again.")
      return
    }
    setPrintProblem(null)
    setPrintedKey(JSON.stringify([key, tickKey]))
    window.open(printedRoundPath(id), '_blank')
  }

  const record = async (f: RoundLeaseFigures): Promise<Failure | null> => {
    try {
      await mutation.mutateAsync({ groundLeaseId: f.groundLeaseId, statementDate: statementDate!, figures: f.figures, delivery })
      return null
    } catch (err) {
      const problem = await getApiProblem(err, 'Failed to record the statement.')
      const lease = leases.find(l => l.groundLeaseId === f.groundLeaseId)
      return { homeowner: lease?.homeownerFullName ?? f.groundLeaseId, reason: problem.message }
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!allReady || !statementDate) return
    setPending(true)
    setFailures([])
    const toRecord = tickedFigures()
    const outcomes = await Promise.all(toRecord.map(record))
    setPending(false)
    const failed = outcomes.filter((r): r is Failure => r !== null)
    const recorded = toRecord.length - failed.length
    if (recorded > 0) success(recorded === 1 ? '1 statement recorded' : `${recorded} statements recorded`)
    if (failed.length === 0) onClose()
    else setFailures(failed)
  }

  const count = ticked.length
  const missing = ticked.length - tickedReady.length
  const isDirty = statementDate !== today
    || Object.keys(entries.indexValues).length > 0
    || Object.keys(entries.marketValues).length > 0
    || (tickedIds !== null && ticked.length !== priceable.length)
  const changedSincePrint = printedKey !== null && printedKey !== JSON.stringify([key, tickKey])

  const priceCell = (id: string) => {
    const lease = leases.find(l => l.groundLeaseId === id)!
    const reason = cannotPriceReason(lease)
    if (reason) return <span style={{ color: 'var(--text-dim)' }}>{reason}</span>
    const result = results.get(id)
    if (result?.price) return <span className="mono">{formatPrice(result.price.price ?? 0)}</span>
    if (result?.errorMessage) return <span style={{ color: 'var(--red)' }}>{result.errorMessage}</span>
    if (!figures.some(f => f.groundLeaseId === id)) return <span style={{ color: 'var(--text-dim)' }}>Enter its figures above</span>
    return <span style={{ color: 'var(--text-dim)' }}>{preview.isError ? '—' : 'Working out…'}</span>
  }

  return (
    <SlideOver
      title="Prepare Statements"
      subtitle={data ? `${leases.length === 1 ? '1 statement' : `${leases.length} statements`} due` : undefined}
      onClose={onClose}
      isDirty={isDirty}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="prepare-resale-price-statements-form"
            disabled={pending || !allReady}
          >
            {pending ? 'Recording…' : count === 1 ? 'Record 1 Statement Sent' : `Record ${count} Statements Sent`}
          </button>
        </>
      }
    >
      {isLoading ? (
        <div className="skeleton-rows"><div className="skeleton-row" /></div>
      ) : leases.length === 0 ? (
        <p style={{ fontSize: 13, color: 'var(--text-dim)' }}>No statements are due.</p>
      ) : (
        <form id="prepare-resale-price-statements-form" onSubmit={handleSubmit}>
          <div className="form-grid">
            <div className="form-field">
              <label className="form-label" htmlFor="statements-date">Statement Date</label>
              <DateInput id="statements-date" className="form-input" value={statementDate} max={today} onChange={setStatementDate} required />
              <div className="form-help">The letters' date, the day the prices are worked out on, and the day they're sent.</div>
            </div>
            {indexTypes.map(({ indexType, baselines }) => (
              <div className="form-field" key={indexType}>
                <label className="form-label" htmlFor={`statements-index-${indexType}`}>{indexType} value on the statement date</label>
                <input
                  id={`statements-index-${indexType}`}
                  className="form-input"
                  type="number"
                  min="0.0001"
                  step="any"
                  value={entries.indexValues[indexType] ?? ''}
                  onChange={e => setIndexValue(indexType, numberOrNull(e.target.value))}
                />
                <div className="form-help">At purchase: {baselines.join(', ')}</div>
              </div>
            ))}
          </div>

          {marketRows.length > 0 && (
            <div className="detail-section">
              <div className="detail-section__title">Market Values</div>
              <div className="form-help" style={{ marginBottom: 8 }}>
                An estimate of what each home would sell for on the statement date, not an appraisal. A blank source uses
                the one above.
              </div>
              <div className="registry-table-wrap">
                <table className="registry-table" aria-label="Market values">
                  <thead>
                    <tr>
                      <th>Homeowner</th>
                      <th>Home</th>
                      <th>Market Value ($)</th>
                      <th>Value Source</th>
                    </tr>
                  </thead>
                  <tbody>
                    {marketRows.map(l => {
                      const id = l.groundLeaseId!
                      const name = l.homeownerFullName ?? 'homeowner'
                      return (
                        <tr key={id}>
                          <td>{l.homeownerFullName ?? '—'}</td>
                          <td>{l.homeLabel ?? '—'}</td>
                          <td>
                            <input
                              className="form-input"
                              type="number"
                              min="0.01"
                              step="0.01"
                              aria-label={`Market value for ${name}`}
                              value={entries.marketValues[id]?.value ?? ''}
                              onChange={e => setMarketValue(id, { value: numberOrNull(e.target.value) })}
                            />
                          </td>
                          <td>
                            <input
                              className="form-input"
                              aria-label={`Value source for ${name}`}
                              value={entries.marketValues[id]?.source ?? ''}
                              placeholder={sources[id] || 'County assessment 2026'}
                              onChange={e => setMarketValue(id, { source: e.target.value })}
                            />
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          <div className="detail-section" aria-live="polite">
            <div className="detail-section__title">Statements</div>
            <div className="form-help" style={{ marginBottom: 8 }}>
              Improvements credits aren't asked here. A lease that needs one gets its statement from the lease.
            </div>
            {current && preview.isError && (
              <p className="form-error" role="alert">Couldn't work out the prices. Try again in a moment.</p>
            )}
            <div className="registry-table-wrap">
              <table className="registry-table" aria-label="Statements to send">
                <thead>
                  <tr>
                    <th>
                      <input
                        type="checkbox"
                        aria-label="Send all"
                        checked={priceable.length > 0 && ticked.length === priceable.length}
                        disabled={priceable.length === 0}
                        onChange={toggleAll}
                      />
                    </th>
                    <th>Homeowner</th>
                    <th>Home</th>
                    <th>Maximum Resale Price</th>
                  </tr>
                </thead>
                <tbody>
                  {leases.map(l => {
                    const id = l.groundLeaseId!
                    const canPrice = !cannotPriceReason(l)
                    return (
                      <tr key={id}>
                        <td>
                          <input
                            type="checkbox"
                            aria-label={`Send to ${l.homeownerFullName ?? 'homeowner'}`}
                            checked={canPrice && !!tickedIds?.has(id)}
                            disabled={!canPrice}
                            onChange={() => toggle(id)}
                          />
                        </td>
                        <td>{l.homeownerFullName ?? '—'}</td>
                        <td>{l.homeLabel ?? '—'}</td>
                        <td>{priceCell(id)}</td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          </div>

          <div className="form-grid">
            <div className="form-field">
              <button type="button" className="btn btn-secondary" onClick={handlePrint} disabled={!allReady}>
                {count === 1 ? 'Print 1 Statement' : `Print ${count} Statements`}
              </button>
              {count > 0 && missing > 0 && (
                <div className="form-help">
                  {missing === 1 ? '1 ticked lease has' : `${missing} ticked leases have`} no price yet. Enter the figures or untick it.
                </div>
              )}
              {printProblem && <div className="form-help" style={{ color: 'var(--red)' }}>{printProblem}</div>}
            </div>
            <div className="form-field">
              <label className="form-label" htmlFor="statements-delivery">Delivery</label>
              <select
                id="statements-delivery"
                className="form-select"
                value={delivery}
                onChange={e => setDelivery(e.target.value as NoticeDelivery)}
              >
                {NOTICE_DELIVERIES.map(d => <option key={d} value={d}>{NOTICE_DELIVERY_LABELS[d]}</option>)}
              </select>
              <div className="form-help">Record them once the letters are posted, emailed or handed over. Untick any not sent.</div>
              {changedSincePrint && (
                <div className="form-help" style={{ color: 'var(--red)' }}>
                  The date, figures or ticks changed after the letters were printed. Print them again so the letters match
                  what is recorded.
                </div>
              )}
            </div>
          </div>

          {failures.length > 0 && (
            <div className="form-error" role="alert" style={{ marginTop: 12 }}>
              {failures.length === 1 ? '1 statement was not recorded:' : `${failures.length} statements were not recorded:`}
              <ul style={{ margin: '4px 0 0 16px' }}>
                {failures.map(f => <li key={f.homeowner}>{f.homeowner}: {f.reason}</li>)}
              </ul>
            </div>
          )}
        </form>
      )}
    </SlideOver>
  )
}
