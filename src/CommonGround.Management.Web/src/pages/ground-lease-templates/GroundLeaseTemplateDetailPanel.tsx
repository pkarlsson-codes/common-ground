import { formatInstant } from '../../lib/calendarDate'
import { useState } from 'react'
import { useGroundLeaseTemplate, useArchiveGroundLeaseTemplate, useRestoreGroundLeaseTemplate } from '../../hooks/groundLeaseTemplates'
import { FORMULA_KIND_LABELS, statusLabel } from '../../hooks/types'
import SlideOver from '../../components/SlideOver'
import StatusBadge from '../../components/StatusBadge'
import { useToast } from '../../hooks/useToast'
import { formatPrice } from '../../lib/currency'
import { getErrorMessage } from '../../lib/getErrorMessage'

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const fmtPct = (n: number) => `${(n * 100).toFixed(1)}%`

interface Props {
  templateId: string
  onClose: () => void
}

export default function GroundLeaseTemplateDetailPanel({ templateId, onClose }: Props) {
  const { data: detail, isLoading } = useGroundLeaseTemplate(templateId)
  const archiveMutation = useArchiveGroundLeaseTemplate()
  const restoreMutation = useRestoreGroundLeaseTemplate()
  const { success } = useToast()
  const [error, setError] = useState<string | null>(null)

  const handleArchive = async () => {
    setError(null)
    try {
      await archiveMutation.mutateAsync(templateId)
      success('Template archived')
      onClose()
    } catch (err) {
      setError(await getErrorMessage(err, 'Failed to archive template.'))
    }
  }

  const handleRestore = async () => {
    setError(null)
    try {
      await restoreMutation.mutateAsync(templateId)
      success('Template restored')
      onClose()
    } catch (err) {
      setError(await getErrorMessage(err, 'Failed to restore template.'))
    }
  }

  const isPending = archiveMutation.isPending || restoreMutation.isPending

  const footer = (
    <div style={{ display: 'flex', gap: 8 }}>
      <button className="btn btn-secondary" onClick={onClose} disabled={isPending}>
        Cancel
      </button>
      {detail?.status === 'Active' ? (
        <button className="btn btn-danger" onClick={handleArchive} disabled={isPending}>
          {archiveMutation.isPending ? 'Archiving…' : 'Archive'}
        </button>
      ) : detail?.status === 'Inactive' ? (
        <button className="btn btn-primary" onClick={handleRestore} disabled={isPending}>
          {restoreMutation.isPending ? 'Restoring…' : 'Restore'}
        </button>
      ) : null}
    </div>
  )

  return (
    <SlideOver
      title={detail?.templateName ?? 'Template'}
      subtitle={detail ? `${detail.termYears} yr · ${statusLabel(detail.status ?? '')}` : undefined}
      onClose={onClose}
      footer={footer}
    >
      {isLoading || !detail ? <SkeletonRows /> : (
        <>
        <div className="detail-grid">
          <div className="detail-field">
            <div className="detail-field__label">Status</div>
            <div className="detail-field__value"><StatusBadge status={detail.status ?? ''} /></div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Formula</div>
            <div className="detail-field__value">{FORMULA_KIND_LABELS[detail.formulaKind ?? ''] ?? detail.formulaKind}</div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Term</div>
            <div className="detail-field__value mono">{detail.termYears} years</div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Option Window</div>
            <div className="detail-field__value mono">{detail.optionWindowDays} days</div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Monthly Lease Fee</div>
            <div className="detail-field__value mono">
              {formatPrice((detail.landUseFee ?? 0) + (detail.repairReserveFee ?? 0))}
              <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>
                {formatPrice(detail.landUseFee ?? 0)} land use + {formatPrice(detail.repairReserveFee ?? 0)} repair reserve
              </div>
            </div>
          </div>
          {detail.appreciationShare != null && (
            <div className="detail-field">
              <div className="detail-field__label">Appreciation Share</div>
              <div className="detail-field__value mono">{fmtPct(detail.appreciationShare)}</div>
            </div>
          )}
          {detail.indexType && (
            <div className="detail-field">
              <div className="detail-field__label">Index Type</div>
              <div className="detail-field__value mono">{detail.indexType}</div>
            </div>
          )}
          {detail.baselineIndexValue != null && (
            <div className="detail-field">
              <div className="detail-field__label">Baseline Index</div>
              <div className="detail-field__value mono">{detail.baselineIndexValue}</div>
            </div>
          )}
          {detail.annualPercentage != null && (
            <div className="detail-field">
              <div className="detail-field__label">Annual %</div>
              <div className="detail-field__value mono">{fmtPct(detail.annualPercentage)}</div>
            </div>
          )}
          {detail.improvementsCap != null && (
            <div className="detail-field">
              <div className="detail-field__label">Improvements Cap</div>
              <div className="detail-field__value mono">{fmtCurrency(detail.improvementsCap)}</div>
            </div>
          )}
          {detail.covenantTerms && (
            <div className="detail-field full-span">
              <div className="detail-field__label">Covenant Terms</div>
              <div className="detail-field__value">{detail.covenantTerms}</div>
            </div>
          )}
          <div className="detail-field">
            <div className="detail-field__label">Created</div>
            <div className="detail-field__value mono">
              {formatInstant(detail.createdAt!)}
            </div>
          </div>
        </div>
        {error && <div className="form-error">{error}</div>}
        </>
      )}
    </SlideOver>
  )
}
