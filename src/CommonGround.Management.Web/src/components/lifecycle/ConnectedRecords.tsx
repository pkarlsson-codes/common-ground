import { useNavigate } from 'react-router-dom'
import StatusBadge from '../StatusBadge'

export interface ConnectedRecord {
  /** What kind of record this is, e.g. "Home" or "Purchase". */
  type: string
  name: string
  status?: string
  meta?: string
  /** The page that shows the record; with `openId`, a list page that opens it in its detail panel. */
  to: string
  key: string
  openId?: string
}

/** Links to every record connected to this one, each showing the record's status. */
export default function ConnectedRecords({ records, isLoading }: { records: ConnectedRecord[]; isLoading?: boolean }) {
  const navigate = useNavigate()

  return (
    <div className="detail-section">
      <div className="detail-section__title">Connected Records</div>
      {isLoading ? (
        <div className="detail-field__value">Loading…</div>
      ) : records.length === 0 ? (
        <div className="detail-field__value">No connected records yet.</div>
      ) : (
        <ul className="connected-records">
          {records.map(record => (
            <li key={record.key}>
              <button
                type="button"
                className="connected-record"
                aria-label={`Open ${record.type}: ${record.name}`}
                onClick={() => navigate(record.to, record.openId ? { state: { openId: record.openId } } : undefined)}
              >
                <span className="connected-record__type">{record.type}</span>
                <span className="connected-record__name">{record.name}</span>
                {record.meta && <span className="connected-record__meta">{record.meta}</span>}
                {record.status && <StatusBadge status={record.status} />}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
