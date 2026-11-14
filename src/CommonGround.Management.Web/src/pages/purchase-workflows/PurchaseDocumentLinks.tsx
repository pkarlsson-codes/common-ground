import type { PurchaseWorkflowResponse } from '../../api'
import { downloadDocument } from '../../hooks/documents'

/** Links to the files recorded on the purchase: the income documents and the signed disclosure. */
export default function PurchaseDocumentLinks({ detail }: { detail: PurchaseWorkflowResponse }) {
  const incomeIds = detail.eligibilityDetails?.incomeDocumentIds ?? []
  const disclosureId = detail.disclosureDetails?.documentId
  if (incomeIds.length === 0 && !disclosureId) return null

  return (
    <div className="detail-section">
      <div className="detail-section__title">Closing Documents</div>
      <ul style={{ listStyle: 'none', margin: 0, padding: 0 }} aria-label="Closing documents">
        {incomeIds.map((id, i) => (
          <li key={id}>
            <button type="button" className="btn btn-ghost btn-sm" onClick={() => downloadDocument(id)}>
              Income document {incomeIds.length > 1 ? i + 1 : ''}
            </button>
          </li>
        ))}
        {disclosureId && (
          <li>
            <button type="button" className="btn btn-ghost btn-sm" onClick={() => downloadDocument(disclosureId)}>
              Signed disclosure
            </button>
          </li>
        )}
      </ul>
    </div>
  )
}
