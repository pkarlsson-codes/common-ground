import { useState, type ChangeEvent } from 'react'
import { Link } from 'react-router-dom'
import type { PortfolioImportResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import PageEyebrow from '../../components/PageEyebrow'
import { useCheckPortfolioImport, useImportPortfolio } from '../../hooks/portfolioImport'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

type Column = { name: string; required: string; values: string }

const DATE = 'A date: 2005-06-01 or 6/1/2005'
const AMOUNT = 'An amount in dollars; $ and , are allowed'

/** The columns of an import file, as the API's PortfolioColumns and PortfolioImportPlanner read them. */
const COLUMNS: readonly Column[] = [
  { name: 'Parcel Number',               required: 'Yes',                 values: 'The county parcel number. Rows with the same number share one land parcel' },
  { name: 'Parcel Address',              required: 'Yes',                 values: 'Text' },
  { name: 'Parcel Acres',                required: 'Yes',                 values: 'A number, such as 0.25' },
  { name: 'Acquired On',                 required: 'Yes',                 values: DATE },
  { name: 'Acquired By',                 required: 'Yes',                 values: 'Purchase, Donation or Transfer' },
  { name: 'Parcel Covenant',             required: 'No',                  values: 'Text' },
  { name: 'Unit',                        required: 'No',                  values: 'Needed when a parcel has more than one home, such as A and B' },
  { name: 'Home Type',                   required: 'Yes',                 values: 'Single Family, Condo Unit, Duplex or Townhouse' },
  { name: 'Bedrooms',                    required: 'Yes',                 values: 'A whole number' },
  { name: 'Bathrooms',                   required: 'Yes',                 values: 'A number, such as 1.5' },
  { name: 'Accessible',                  required: 'No',                  values: 'Yes or No; blank means No' },
  { name: 'Income Limit',                required: 'No',                  values: 'A percent of AMI, such as 80, or None; blank means 80' },
  { name: 'Square Feet',                 required: 'Yes',                 values: 'A number' },
  { name: 'Year Built',                  required: 'Yes',                 values: 'A year, such as 1952' },
  { name: 'Assessed Value',              required: 'Yes',                 values: AMOUNT },
  { name: 'Assessed On',                 required: 'Yes',                 values: DATE },
  { name: 'Homeowner Name',              required: 'No',                  values: 'Leave blank for a home without a lease; the columns below are then ignored' },
  { name: 'Homeowner Email',             required: 'With a homeowner',    values: 'An email address' },
  { name: 'Homeowner Phone',             required: 'No',                  values: 'A phone number' },
  { name: 'Lease Template',              required: 'With a homeowner',    values: 'The name of one of the trust\'s active ground lease templates. It sets the term, option window, covenant, resale formula and interest' },
  { name: 'Lease Start',                 required: 'With a homeowner',    values: DATE },
  { name: 'Base Price',                  required: 'With a homeowner',    values: AMOUNT },
  { name: 'Appraised Value at Purchase', required: 'With a homeowner',    values: AMOUNT },
  { name: 'Land Use Fee',                required: 'No',                  values: 'Monthly amount; blank uses the template\'s' },
  { name: 'Repair Reserve Fee',          required: 'No',                  values: 'Monthly amount; blank uses the template\'s' },
  { name: 'Fees Tracked From',           required: 'With a homeowner',    values: 'The date CommonGround starts counting fees owed, not before Lease Start. Months before it aren\'t charged' },
  { name: 'Amount Owed Then',            required: 'With a homeowner',    values: 'Fees and interest owed on that date; enter 0 if paid up' },
]

/** Imports the trust's existing land parcels, homes and ground leases from one CSV file. */
export default function PortfolioImportPage() {
  const check = useCheckPortfolioImport()
  const importer = useImportPortfolio()
  const { success } = useToast()
  const [file, setFile] = useState<File | null>(null)
  // The text that was checked; Import sends exactly this.
  const [csv, setCsv] = useState<string | null>(null)
  const [result, setResult] = useState<PortfolioImportResponse | null>(null)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)

  const onFile = (e: ChangeEvent<HTMLInputElement>) => {
    setFile(e.target.files?.[0] ?? null)
    setCsv(null)
    setResult(null)
    setProblem(null)
  }

  const onCheck = async () => {
    if (file === null) return
    setProblem(null)
    setResult(null)
    let text: string
    try {
      text = await file.text()
    } catch {
      setProblem({ message: 'Could not read the file. It may have changed since you chose it; choose it again.' })
      return
    }
    try {
      setResult(await check.mutateAsync(text))
      setCsv(text)
    } catch (err) {
      setProblem(await getApiProblem(err, 'Could not check the file.'))
    }
  }

  const onImport = async () => {
    if (csv === null) return
    setProblem(null)
    try {
      const imported = await importer.mutateAsync(csv)
      setResult(imported)
      if (imported.imported) success(`Imported ${imported.homesToCreate ?? 0} homes`)
    } catch (err) {
      setProblem(await getApiProblem(err, 'The import failed. Choose the same file and import again to finish it.'))
    }
  }

  const errors = result?.errors ?? []
  const nothingNew = result !== null && !result.parcelsToCreate && !result.homesToCreate && !result.leasesToCreate
  const busy = check.isPending || importer.isPending

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <PageEyebrow />
          <h1 className="page-header__title">Import Portfolio</h1>
        </div>
        <a className="btn btn-ghost" href="/portfolio-import-template.csv" download>Download template</a>
      </div>

      <div className="detail-section">
        <div className="detail-section__title">File</div>
        <p style={{ marginTop: 0 }}>
          One row per home, saved as CSV. Importing the same file twice creates nothing twice: parcels, homes and
          leases that already exist are skipped.
        </p>
        <div style={{ display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap' }}>
          <input
            type="file"
            accept=".csv,text/csv"
            aria-label="Import file"
            className="form-input"
            style={{ cursor: 'pointer', maxWidth: 360 }}
            // Cleared on click so choosing the same file again, after fixing it, still fires onChange.
            onClick={e => { e.currentTarget.value = '' }}
            onChange={onFile}
          />
          <button className="btn btn-primary" disabled={file === null || busy} onClick={() => { void onCheck() }}>
            {check.isPending ? 'Checking…' : 'Check File'}
          </button>
        </div>
        <ApiProblemAlert problem={problem} />
      </div>

      {result && errors.length > 0 && (
        <div className="detail-section">
          <div className="detail-section__title">Errors</div>
          {result.stoppedAtRow ? (
            <p style={{ marginTop: 0 }}>
              The import stopped at row {result.stoppedAtRow}. Rows before it were imported. Fix that row, choose the
              file again and import it; what was imported is skipped.
            </p>
          ) : (
            <p style={{ marginTop: 0 }}>
              Nothing was imported. Fix {errors.length === 1 ? 'this error' : `these ${errors.length} errors`} in{' '}
              {file?.name ?? 'the file'}, choose it again and check it.
            </p>
          )}
          <table className="registry-table" aria-label="Errors">
            <thead><tr><th>Row</th><th>Column</th><th>Error</th></tr></thead>
            <tbody>
              {errors.map((e, i) => (
                <tr key={i}>
                  <td><span className="cell-mono">{e.row}</span></td>
                  <td>{e.column ?? <span style={{ color: 'var(--text-dim)' }}>—</span>}</td>
                  <td>{e.message}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {result && errors.length === 0 && (
        <div className="detail-section">
          <div className="detail-section__title">{result.imported ? 'Imported' : 'Ready to Import'}</div>
          <p style={{ marginTop: 0 }} aria-label="Import summary">
            {result.imported ? 'Created' : 'Will create'} {count(result.parcelsToCreate, 'land parcel')},{' '}
            {count(result.homesToCreate, 'home')} and {count(result.leasesToCreate, 'ground lease')}.
            {skipped(result)}
          </p>
          {result.imported ? (
            <div style={{ display: 'flex', gap: 8 }}>
              <Link className="btn btn-ghost" to="/land-parcels">Land Parcels</Link>
              <Link className="btn btn-ghost" to="/homes">Homes</Link>
              <Link className="btn btn-ghost" to="/ground-leases">Ground Leases</Link>
            </div>
          ) : (
            <button className="btn btn-primary" disabled={busy || nothingNew || csv === null} onClick={() => { void onImport() }}>
              {importer.isPending ? 'Importing…' : 'Import'}
            </button>
          )}
        </div>
      )}

      <div className="detail-section">
        <div className="detail-section__title">Columns</div>
        <p style={{ marginTop: 0 }}>
          Headers must match these names; case and column order don&apos;t matter. Columns that are not required may
          be left out.
        </p>
        <table className="registry-table" aria-label="Columns">
          <thead><tr><th>Column</th><th>Required</th><th>Values</th></tr></thead>
          <tbody>
            {COLUMNS.map(c => (
              <tr key={c.name}><td>{c.name}</td><td>{c.required}</td><td>{c.values}</td></tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

function count(n: number | undefined, noun: string): string {
  const value = n ?? 0
  return `${value} ${noun}${value === 1 ? '' : 's'}`
}

function skipped(r: PortfolioImportResponse): string {
  const parts = [
    r.parcelsExisting ? count(r.parcelsExisting, 'land parcel') : null,
    r.homesExisting ? count(r.homesExisting, 'home') : null,
    r.leasesExisting ? count(r.leasesExisting, 'ground lease') : null,
  ].filter((p): p is string => p !== null)

  return parts.length === 0 ? '' : ` Already there and skipped: ${parts.join(', ')}.`
}
