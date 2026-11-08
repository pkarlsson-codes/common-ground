import { useState } from 'react'
import { useHome, useHomeAssessments, useRecordHomeAssessment } from '../../hooks/homes'
import { HOME_TYPE_LABELS, type RecordHomeAssessmentRequest } from '../../hooks/types'
import SlideOver from '../../components/SlideOver'
import StatusBadge from '../../components/StatusBadge'
import { homeLabel } from '../../lib/lifecycle'
import DocumentList from '../../components/DocumentList'
import { useToast } from '../../hooks/useToast'
import DevelopmentProjectsSection from '../land-parcels/DevelopmentProjectsSection'
import { HomeConnectedRecords, HomeLifecycle } from './HomeLifecycle'
import { trustToday } from '../../lib/calendarDate'
import { DateInput } from '../../components/DateInput'

type Tab = 'details' | 'documents'

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const fmtNumber = (n: number) => new Intl.NumberFormat('en-US').format(n)

const emptyAssessment = (): RecordHomeAssessmentRequest => ({
  assessedValue: 0,
  assessmentDate: trustToday(),
  appraiserName: '',
  conditionNotes: '',
})

interface Props {
  homeId: string
  onClose: () => void
}

export default function HomeDetailPanel({ homeId, onClose }: Props) {
  const { data: detail, isLoading: detailLoading } = useHome(homeId)
  const { data: assessments, isLoading: assessmentsLoading } = useHomeAssessments(homeId)
  const assessmentMutation = useRecordHomeAssessment()
  const [assessment, setAssessment] = useState<RecordHomeAssessmentRequest>(emptyAssessment)
  const [showAssessmentForm, setShowAssessmentForm] = useState(false)
  const [activeTab, setActiveTab] = useState<Tab>('details')
  const { success } = useToast()

  const sortedAssessments = assessments?.items
    ? [...assessments.items].sort((a, b) => (b.assessmentDate ?? '').localeCompare(a.assessmentDate ?? ''))
    : []

  const handleAssessment = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await assessmentMutation.mutateAsync({
        homeId,
        req: {
          ...assessment,
          assessedValue: Number(assessment.assessedValue),
          appraiserName: assessment.appraiserName || undefined,
          conditionNotes: assessment.conditionNotes || undefined,
        },
      })
      setAssessment(emptyAssessment())
      setShowAssessmentForm(false)
      success('Assessment recorded')
    } catch { /* error shown inline */ }
  }

  return (
    <SlideOver
      title={detail ? (detail.label || HOME_TYPE_LABELS[detail.homeType ?? ''] || '…') : '…'}
      subtitle={detail ? `${HOME_TYPE_LABELS[detail.homeType ?? ''] ?? detail.homeType} · ${detail.bedrooms} bd · ${Number(detail.bathrooms).toFixed(1)} ba · ${fmtNumber(Number(detail.squareFootage))} sq ft` : undefined}
      onClose={onClose}
    >
      {detailLoading || !detail ? <SkeletonRows /> : (
        <>
          <HomeLifecycle
            home={detail}
            moreActions={[
              { label: 'Add New Assessment', onClick: () => { setActiveTab('details'); setShowAssessmentForm(true) } },
              { label: 'Upload Document', onClick: () => setActiveTab('documents') },
            ]}
          />

          <div style={{
            display: 'flex',
            gap: 4,
            borderBottom: '1px solid var(--border)',
            marginBottom: 16,
          }}>
            {(['details', 'documents'] as Tab[]).map(tab => (
              <button
                key={tab}
                type="button"
                onClick={() => setActiveTab(tab)}
                style={{
                  padding: '6px 16px',
                  fontSize: 12,
                  fontFamily: 'var(--font-mono)',
                  letterSpacing: '0.06em',
                  textTransform: 'uppercase',
                  background: 'none',
                  border: 'none',
                  cursor: 'pointer',
                  color: activeTab === tab ? 'var(--amber-bright)' : 'var(--text-secondary)',
                  borderBottom: activeTab === tab ? '2px solid var(--amber)' : '2px solid transparent',
                  marginBottom: -1,
                  transition: 'var(--transition)',
                }}
              >
                {tab}
              </button>
            ))}
          </div>

          {activeTab === 'documents' ? (
            <DocumentList entityType="Building" entityId={homeId} />
          ) : (
            <>
              <div className="detail-grid">
                <div className="detail-field">
                  <div className="detail-field__label">Status</div>
                  <div className="detail-field__value"><StatusBadge status={homeLabel(detail.occupancy, detail.saleStage)} /></div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Accessible</div>
                  <div className="detail-field__value">{detail.isAccessible ? 'Yes' : 'No'}</div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Year Built</div>
                  <div className="detail-field__value mono">{detail.yearBuilt}</div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Assessed Value</div>
                  <div className="detail-field__value mono">{fmtCurrency(Number(detail.assessedValue))}</div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Last Assessed</div>
                  <div className="detail-field__value mono">{detail.lastAssessedDate ?? '—'}</div>
                </div>
                {detail.conditionNotes && (
                  <div className="detail-field full-span">
                    <div className="detail-field__label">Condition Notes</div>
                    <div className="detail-field__value">{detail.conditionNotes}</div>
                  </div>
                )}
                <div className="detail-field full-span">
                  <div className="detail-field__label">Registered At</div>
                  <div className="detail-field__value mono">
                    {new Date(detail.registeredAt).toLocaleString()}
                  </div>
                </div>
              </div>

              <HomeConnectedRecords home={detail} />

              {detail.landParcelId && (
                <DevelopmentProjectsSection
                  parcelId={detail.landParcelId}
                  initialHomeIds={[homeId]}
                />
              )}

              <div
                className="detail-section"
                style={{
                  border: '1px solid var(--border)',
                  borderRadius: 12,
                  padding: 12,
                  background: 'var(--surface-1)',
                  display: 'grid',
                  gap: 12,
                }}
              >
                <div
                  style={{
                    paddingBottom: showAssessmentForm ? 12 : 0,
                    borderBottom: showAssessmentForm ? '1px solid var(--border)' : 'none',
                  }}
                >
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: showAssessmentForm ? 12 : 0 }}>
                    <div className="detail-section__title" style={{ marginBottom: 0 }}>Assessments</div>
                    <button
                      type="button"
                      className="btn btn-ghost btn-sm"
                      onClick={() => setShowAssessmentForm(s => !s)}
                    >
                      {showAssessmentForm ? 'Hide Form' : 'Add New Assessment'}
                    </button>
                  </div>

                  {showAssessmentForm && (
                    <form onSubmit={handleAssessment}>
                      <div className="form-grid" style={{ marginBottom: 12 }}>
                        <div className="form-field">
                          <label className="form-label">Assessed Value ($)</label>
                          <input
                            className="form-input"
                            type="number"
                            min="0"
                            step="1"
                            value={assessment.assessedValue || ''}
                            onChange={e => setAssessment(a => ({ ...a, assessedValue: parseFloat(e.target.value) || 0 }))}
                            required
                          />
                        </div>
                        <div className="form-field">
                          <label className="form-label">Assessment Date</label>
                          <DateInput
                            className="form-input"
                            value={assessment.assessmentDate}
                            onChange={v => setAssessment(a => ({ ...a, assessmentDate: v }))}
                            required
                          />
                        </div>
                        <div className="form-field full">
                          <label className="form-label">Appraiser Name (optional)</label>
                          <input
                            className="form-input"
                            type="text"
                            value={assessment.appraiserName}
                            onChange={e => setAssessment(a => ({ ...a, appraiserName: e.target.value }))}
                          />
                        </div>
                        <div className="form-field full">
                          <label className="form-label">Condition Notes (optional)</label>
                          <textarea
                            className="form-textarea"
                            value={assessment.conditionNotes}
                            onChange={e => setAssessment(a => ({ ...a, conditionNotes: e.target.value }))}
                            placeholder="Observations from the assessment…"
                          />
                        </div>
                      </div>
                      <button
                        type="submit"
                        className="btn btn-secondary"
                        disabled={assessmentMutation.isPending || !assessment.assessedValue}
                      >
                        {assessmentMutation.isPending ? 'Recording…' : 'Record Assessment'}
                      </button>
                      {assessmentMutation.isError && (
                        <div className="form-error">Failed to record assessment.</div>
                      )}
                    </form>
                  )}
                </div>
                <div style={{ display: 'grid', gap: 6 }}>
                  {assessmentsLoading ? (
                    <div className="detail-field__value">Loading assessments…</div>
                  ) : !sortedAssessments.length ? (
                    <div className="detail-field__value">No assessments yet.</div>
                  ) : (
                    <div style={{ display: 'grid', gap: 6 }}>
                      {sortedAssessments.map(item => (
                        <div
                          key={item.id ?? `${item.assessmentDate ?? ''}-${item.assessedValue ?? 0}`}
                          style={{
                            display: 'grid',
                            gridTemplateColumns: '120px 1fr auto',
                            gap: 10,
                            alignItems: 'center',
                            padding: '8px 10px',
                            borderRadius: 8,
                            border: '1px solid var(--border)',
                            background: 'var(--surface-2)',
                          }}
                        >
                          <span className="detail-field__label" style={{ marginBottom: 0 }}>
                            {item.assessmentDate ?? '—'}
                          </span>
                          <span className="detail-field__value mono" style={{ marginBottom: 0 }}>
                            {fmtCurrency(Number(item.assessedValue ?? 0))}
                          </span>
                          <span className="detail-field__value" style={{ marginBottom: 0, whiteSpace: 'nowrap' }}>
                            {item.appraiserName ?? '—'}
                          </span>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </>
          )}
        </>
      )}
    </SlideOver>
  )
}
