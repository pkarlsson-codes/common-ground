import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import type { FunderReportResaleResponse, FunderReportSaleResponse } from '../../api'
import { DateInput } from '../../components/DateInput'
import PageEyebrow from '../../components/PageEyebrow'
import { useFunderReport } from '../../hooks/reports'
import { formatCalendarDate, parseCalendarDate, trustToday, yearOf, type CalendarDate } from '../../lib/calendarDate'
import { downloadCsv, toCsv } from '../../lib/csv'
import { formatCurrency } from '../../lib/currency'
import { incomeBandLabel } from '../../lib/incomeBand'

const NOT_RECORDED = 'Not recorded'

const money = (n: number | null | undefined) => (n == null ? NOT_RECORDED : formatCurrency(n))

const bandOf = (sale: FunderReportSaleResponse) =>
  incomeBandLabel({ overPercentOfAmi: sale.incomeBandOverPercentOfAmi, upToPercentOfAmi: sale.incomeBandUpToPercentOfAmi }) ?? NOT_RECORDED

const SALES_HEADER = ['Home', 'Start Date', 'Price', 'Appraised Value', 'Affordability Given', 'Household Size', 'Verified Income', 'Income Band']
const salesCsv = (sales: FunderReportSaleResponse[]) => toCsv(SALES_HEADER, sales.map(s => [
  s.homeLabel, s.leaseStartDate, s.price, s.appraisedValue, s.affordabilityGiven, s.householdSize, s.annualIncome, bandOf(s),
]))

const RESALES_HEADER = ['Home', 'Closing Date', 'Original Price', 'Appraised Value', 'Purchase Option Price', 'Price Paid', 'Affordability Kept', 'Seller Gain']
const resalesCsv = (resales: FunderReportResaleResponse[]) => toCsv(RESALES_HEADER, resales.map(r => [
  r.homeLabel, r.closingDate, r.originalPrice, r.appraisedValue, r.purchaseOptionPrice, r.pricePaid, r.affordabilityKept, r.sellerGain,
]))

/** Jan 1 to Dec 31 of the year before the trust's today. */
const lastCalendarYear = () => {
  const year = yearOf(trustToday()) - 1
  return { from: parseCalendarDate(`${year}-01-01`), to: parseCalendarDate(`${year}-12-31`) }
}

/** What the trust held, whom it sold to and what it bought back in a period, for its funders. */
export default function FunderReportPage() {
  const navigate = useNavigate()
  const [period, setPeriod] = useState<{ from: CalendarDate; to: CalendarDate }>(lastCalendarYear)
  const { from, to } = period
  const toBeforeFrom = to < from
  const { data, isLoading, isError, refetch } = useFunderReport(from, to, !toBeforeFrom)

  const sales = data?.sales ?? []
  const resales = data?.resales ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <PageEyebrow />
          <h1 className="page-header__title">Funder Report</h1>
        </div>
      </div>

      <div className="filter-bar report-period">
        <div className="form-field">
          <label className="form-label" htmlFor="funder-report-from">From</label>
          <DateInput id="funder-report-from" className="form-input" required value={from}
            onChange={v => setPeriod(p => ({ ...p, from: v }))} />
        </div>
        <div className="form-field">
          <label className="form-label" htmlFor="funder-report-to">To</label>
          <DateInput id="funder-report-to" className="form-input" required value={to}
            onChange={v => setPeriod(p => ({ ...p, to: v }))} />
        </div>
      </div>

      {toBeforeFrom && <div className="form-error" role="alert">To must be on or after From</div>}

      {!toBeforeFrom && isLoading && (
        <div className="skeleton-rows">
          {Array.from({ length: 4 }).map((_, i) => <div key={i} className="skeleton-row" />)}
        </div>
      )}

      {!toBeforeFrom && isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load the funder report</div>
          <div className="error-state__sub">Try again in a moment. If it keeps failing, contact support.</div>
          <div style={{ marginTop: 10 }}>
            <button className="btn btn-ghost btn-sm" type="button" onClick={() => { void refetch() }}>Retry</button>
          </div>
        </div>
      )}

      {!toBeforeFrom && data && (
        <>
          <section className="report-section" aria-labelledby="funder-report-portfolio">
            <div className="report-section__header">
              <h2 className="report-section__title" id="funder-report-portfolio">Portfolio on {formatCalendarDate(to)}</h2>
            </div>
            <div className="registry-table-wrap">
              <table className="registry-table">
                <thead>
                  <tr><th>Homes</th><th>Under a Ground Lease</th><th>Held by the Trust or for Sale</th></tr>
                </thead>
                <tbody>
                  <tr>
                    <td className="cell-mono">{data.portfolio?.homes}</td>
                    <td className="cell-mono">{data.portfolio?.underGroundLease}</td>
                    <td className="cell-mono">{data.portfolio?.heldOrForSale}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <section className="report-section" aria-labelledby="funder-report-sales">
            <div className="report-section__header">
              <h2 className="report-section__title" id="funder-report-sales">Sales</h2>
              <button className="btn btn-ghost btn-sm" type="button" disabled={sales.length === 0}
                onClick={() => downloadCsv(`funder-report-sales-${from}-${to}.csv`, salesCsv(sales))}>
                Download CSV
              </button>
            </div>
            {sales.length === 0 ? (
              <div className="empty-state"><div className="empty-state__title">No sales in this period</div></div>
            ) : (
              <div className="registry-table-wrap">
                <table className="registry-table">
                  <thead>
                    <tr>{SALES_HEADER.map(h => <th key={h}>{h}</th>)}</tr>
                  </thead>
                  <tbody>
                    {sales.map(s => (
                      <tr key={s.groundLeaseId} style={{ cursor: 'pointer' }}
                        onClick={() => navigate('/ground-leases', { state: { openId: s.groundLeaseId } })}>
                        <td>{s.homeLabel}</td>
                        <td className="cell-mono">{formatCalendarDate(s.leaseStartDate!)}</td>
                        <td className="cell-mono">{money(s.price)}</td>
                        <td className="cell-mono">{money(s.appraisedValue)}</td>
                        <td className="cell-mono">{money(s.affordabilityGiven)}</td>
                        <td className="cell-mono">{s.householdSize ?? NOT_RECORDED}</td>
                        <td className="cell-mono">{money(s.annualIncome)}</td>
                        <td>{bandOf(s)}</td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot>
                    <tr>
                      <td colSpan={4}>Total affordability given</td>
                      <td className="cell-mono">{money(data.totalAffordabilityGiven)}</td>
                      <td colSpan={3} />
                    </tr>
                  </tfoot>
                </table>
              </div>
            )}
          </section>

          <section className="report-section" aria-labelledby="funder-report-bands">
            <div className="report-section__header">
              <h2 className="report-section__title" id="funder-report-bands">Households by Income Band</h2>
            </div>
            {(data.incomeBands ?? []).length === 0 ? (
              <div className="empty-state"><div className="empty-state__title">No sales in this period</div></div>
            ) : (
              <div className="registry-table-wrap">
                <table className="registry-table">
                  <thead>
                    <tr><th>Income Band</th><th>Households</th></tr>
                  </thead>
                  <tbody>
                    {(data.incomeBands ?? []).map(b => {
                      const label = incomeBandLabel(b) ?? NOT_RECORDED
                      return (
                        <tr key={label}>
                          <td>{label}</td>
                          <td className="cell-mono">{b.count}</td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </section>

          <section className="report-section" aria-labelledby="funder-report-resales">
            <div className="report-section__header">
              <h2 className="report-section__title" id="funder-report-resales">Resales</h2>
              <button className="btn btn-ghost btn-sm" type="button" disabled={resales.length === 0}
                onClick={() => downloadCsv(`funder-report-resales-${from}-${to}.csv`, resalesCsv(resales))}>
                Download CSV
              </button>
            </div>
            {resales.length === 0 ? (
              <div className="empty-state"><div className="empty-state__title">No resales in this period</div></div>
            ) : (
              <div className="registry-table-wrap">
                <table className="registry-table">
                  <thead>
                    <tr>{RESALES_HEADER.map(h => <th key={h}>{h}</th>)}</tr>
                  </thead>
                  <tbody>
                    {resales.map(r => (
                      <tr key={r.resaleId} style={{ cursor: 'pointer' }}
                        onClick={() => navigate('/resales', { state: { openId: r.resaleId } })}>
                        <td>{r.homeLabel}</td>
                        <td className="cell-mono">{formatCalendarDate(r.closingDate!)}</td>
                        <td className="cell-mono">{money(r.originalPrice)}</td>
                        <td className="cell-mono">{money(r.appraisedValue)}</td>
                        <td className="cell-mono">{money(r.purchaseOptionPrice)}</td>
                        <td className="cell-mono">{money(r.pricePaid)}</td>
                        <td className="cell-mono">{money(r.affordabilityKept)}</td>
                        <td className="cell-mono">{money(r.sellerGain)}</td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot>
                    <tr>
                      <td colSpan={6}>Total affordability kept</td>
                      <td className="cell-mono">{money(data.totalAffordabilityKept)}</td>
                      <td />
                    </tr>
                  </tfoot>
                </table>
              </div>
            )}
          </section>
        </>
      )}
    </div>
  )
}
