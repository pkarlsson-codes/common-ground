import { useState } from 'react'
import {
  useDocument,
  useEntityDocuments,
  useUploadDocument,
  useAddDocumentVersion,
  useSoftDeleteDocument,
  downloadDocument,
} from '../hooks/documents'
import { useToast } from '../hooks/useToast'

const fmtSize = (bytes: number): string => {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

const fmtDate = (d: Date | string) => {
  const date = d instanceof Date ? d : new Date(d as string)
  return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
}

const SENSITIVITY_OPTIONS = ['Public', 'Internal', 'Confidential', 'Restricted']
const RETENTION_OPTIONS = ['Transient', 'Standard3yr', 'LongTerm7yr', 'Permanent']

interface Props {
  entityType: string
  entityId: string
}

interface UploadForm {
  title: string
  sensitivity: string
  retention: string
  file: File | null
}

const emptyForm = (): UploadForm => ({
  title: '',
  sensitivity: 'Internal',
  retention: 'Standard3yr',
  file: null,
})

export default function DocumentList({ entityType, entityId }: Props) {
  const [page, setPage] = useState(1)
  const [showUpload, setShowUpload] = useState(false)
  const [form, setForm] = useState<UploadForm>(emptyForm)
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [dragOver, setDragOver] = useState(false)
  const [expandedId, setExpandedId] = useState<string | null>(null)

  const { data, isLoading, isError } = useEntityDocuments(entityType, entityId, page)
  const uploadMutation = useUploadDocument()
  const deleteMutation = useSoftDeleteDocument()
  const { success, error: toastError } = useToast()

  const handleFileDrop = (e: React.DragEvent) => {
    e.preventDefault()
    setDragOver(false)
    const file = e.dataTransfer.files[0]
    if (!file) return
    setForm(f => ({ ...f, file, title: f.title || file.name.replace(/\.[^.]+$/, '') }))
    setShowUpload(true)
  }

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return
    setForm(f => ({ ...f, file, title: f.title || file.name.replace(/\.[^.]+$/, '') }))
  }

  const handleUpload = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!form.file) return

    try {
      await uploadMutation.mutateAsync({
        metadata: {
          title: form.title || form.file.name,
          sensitivity: form.sensitivity,
          retention: form.retention,
          initialAttachment: { entityType, entityId },
        },
        file: form.file,
      })
      setForm(emptyForm())
      setShowUpload(false)
      success('Document uploaded')
    } catch {
      toastError('Upload failed')
    }
  }

  const handleDelete = async (documentId: string, title: string) => {
    if (!window.confirm(`Delete "${title}"? This can be restored later.`)) return
    setDeletingId(documentId)
    try {
      await deleteMutation.mutateAsync({
        documentId,
      })
      success('Document removed')
    } catch {
      toastError('Delete failed')
    } finally {
      setDeletingId(null)
    }
  }

  const items = data?.items ?? []
  const totalPages = data?.totalPages ?? 1

  return (
    <div style={{ display: 'grid', gap: 12 }}>
      {/* Header row */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <div className="detail-section__title" style={{ marginBottom: 0 }}>Documents</div>
        <button
          type="button"
          className="btn btn-ghost btn-sm"
          onClick={() => setShowUpload(s => !s)}
        >
          {showUpload ? 'Cancel' : 'Upload Document'}
        </button>
      </div>

      {/* Upload form / drop zone */}
      {showUpload && (
        <div
          style={{
            border: `1px dashed ${dragOver ? 'var(--amber)' : 'var(--border-bright)'}`,
            borderRadius: 8,
            padding: 16,
            background: dragOver ? 'var(--amber-dim)' : 'var(--surface-1)',
            transition: 'var(--transition)',
          }}
          onDragOver={e => { e.preventDefault(); setDragOver(true) }}
          onDragLeave={() => setDragOver(false)}
          onDrop={handleFileDrop}
        >
          <form onSubmit={handleUpload}>
            <div className="form-grid" style={{ marginBottom: 12 }}>
              <div className="form-field full">
                <label className="form-label">File</label>
                <input
                  type="file"
                  className="form-input"
                  style={{ cursor: 'pointer' }}
                  onChange={handleFileSelect}
                  required={!form.file}
                />
                {form.file && (
                  <div style={{ fontSize: 11, color: 'var(--text-secondary)', marginTop: 4 }}>
                    {form.file.name} ({fmtSize(form.file.size)})
                  </div>
                )}
                {!form.file && (
                  <div style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 4 }}>
                    Or drag and drop a file here
                  </div>
                )}
              </div>
              <div className="form-field full">
                <label className="form-label">Title</label>
                <input
                  className="form-input"
                  type="text"
                  value={form.title}
                  onChange={e => setForm(f => ({ ...f, title: e.target.value }))}
                  placeholder="Document title"
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Sensitivity</label>
                <select
                  className="form-select"
                  value={form.sensitivity}
                  onChange={e => setForm(f => ({ ...f, sensitivity: e.target.value }))}
                >
                  {SENSITIVITY_OPTIONS.map(s => (
                    <option key={s} value={s}>{s}</option>
                  ))}
                </select>
              </div>
              <div className="form-field">
                <label className="form-label">Retention</label>
                <select
                  className="form-select"
                  value={form.retention}
                  onChange={e => setForm(f => ({ ...f, retention: e.target.value }))}
                >
                  {RETENTION_OPTIONS.map(r => (
                    <option key={r} value={r}>{r}</option>
                  ))}
                </select>
              </div>
            </div>
            <div style={{ display: 'flex', gap: 8 }}>
              <button
                type="submit"
                className="btn btn-secondary"
                disabled={uploadMutation.isPending || !form.file || !form.title}
              >
                {uploadMutation.isPending ? 'Uploading…' : 'Upload'}
              </button>
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                onClick={() => { setForm(emptyForm()); setShowUpload(false) }}
              >
                Cancel
              </button>
            </div>
            {uploadMutation.isError && (
              <div className="form-error" style={{ marginTop: 8 }}>Upload failed. Please try again.</div>
            )}
          </form>
        </div>
      )}

      {/* Document list */}
      {isLoading ? (
        <div className="skeleton-rows">
          {Array.from({ length: 3 }).map((_, i) => <div key={i} className="skeleton-row" />)}
        </div>
      ) : isError ? (
        <div className="form-error">Failed to load documents.</div>
      ) : items.length === 0 ? (
        <div style={{ color: 'var(--text-secondary)', fontSize: 13, padding: '8px 0' }}>
          No documents attached yet.
        </div>
      ) : (
        <div style={{ display: 'grid', gap: 6 }}>
          {items.map(item => (
            <div key={item.documentId} style={{ display: 'grid', gap: 0 }}>
              <div
                style={{
                  display: 'grid',
                  gridTemplateColumns: '1fr auto',
                  gap: 12,
                  padding: '10px 12px',
                  borderRadius: expandedId === item.documentId ? '8px 8px 0 0' : 8,
                  border: '1px solid var(--border)',
                  borderBottom: expandedId === item.documentId ? 'none' : undefined,
                  background: 'var(--surface-2)',
                }}
              >
                <div style={{ display: 'grid', gap: 4 }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    <span style={{ fontWeight: 600, color: 'var(--text-bright)', fontSize: 13 }}>
                      {item.title}
                    </span>
                    {item.role && (
                      <span style={{ fontSize: 10, color: 'var(--text-dim)', fontFamily: 'var(--font-mono)' }}>
                        {item.role}
                      </span>
                    )}
                  </div>
                  <div style={{ display: 'flex', gap: 12, fontSize: 11, color: 'var(--text-secondary)' }}>
                    <span style={{ fontFamily: 'var(--font-mono)' }}>{item.currentFileName}</span>
                    <span>{fmtSize(item.currentSizeBytes ?? 0)}</span>
                    <span>{item.attachedAt ? fmtDate(item.attachedAt) : ''}</span>
                  </div>
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                  <button
                    type="button"
                    className="btn btn-ghost btn-sm"
                    onClick={() => setExpandedId(expandedId === item.documentId ? null : (item.documentId ?? null))}
                    title="Version history"
                    style={{ fontSize: 11 }}
                  >
                    Versions
                  </button>
                  <button
                    type="button"
                    className="btn btn-ghost btn-sm"
                    onClick={() => downloadDocument(item.documentId ?? '')}
                    title="Download"
                  >
                    Download
                  </button>
                  <button
                    type="button"
                    className="btn btn-ghost btn-sm"
                    style={{ color: 'var(--red)' }}
                    onClick={() => handleDelete(item.documentId ?? '', item.title ?? '')}
                    disabled={deletingId === item.documentId}
                    title="Remove"
                  >
                    {deletingId === item.documentId ? '…' : 'Remove'}
                  </button>
                </div>
              </div>
              {expandedId === item.documentId && (
                <DocumentVersionPanel documentId={item.documentId ?? ''} entityId={entityId} />
              )}
            </div>
          ))}
        </div>
      )}

      {/* Pagination */}
      {(totalPages ?? 1) > 1 && (
        <div style={{ display: 'flex', gap: 8, justifyContent: 'center', marginTop: 8 }}>
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            disabled={page <= 1}
            onClick={() => setPage(p => p - 1)}
          >
            Prev
          </button>
          <span style={{ fontSize: 12, color: 'var(--text-secondary)', lineHeight: '28px' }}>
            {page} / {totalPages}
          </span>
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            disabled={page >= (totalPages ?? 1)}
            onClick={() => setPage(p => p + 1)}
          >
            Next
          </button>
        </div>
      )}
    </div>
  )
}

// ── Version history panel ─────────────────────────────────────────────────────

interface VersionPanelProps {
  documentId: string
  entityId: string
}

function DocumentVersionPanel({ documentId }: VersionPanelProps) {
  const { data: doc, isLoading } = useDocument(documentId)
  const addVersionMutation = useAddDocumentVersion()
  const { success, error: toastError } = useToast()
  const [showVersionUpload, setShowVersionUpload] = useState(false)
  const [versionFile, setVersionFile] = useState<File | null>(null)
  const [versionNote, setVersionNote] = useState('')
  const [dragOver, setDragOver] = useState(false)

  const handleVersionUpload = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!versionFile) return
    try {
      await addVersionMutation.mutateAsync({ documentId, file: versionFile, note: versionNote || undefined })
      setVersionFile(null)
      setVersionNote('')
      setShowVersionUpload(false)
      success('New version uploaded')
    } catch {
      toastError('Version upload failed')
    }
  }

  return (
    <div
      style={{
        padding: '10px 12px',
        borderRadius: '0 0 8px 8px',
        border: '1px solid var(--border)',
        borderTop: '1px solid var(--border-bright)',
        background: 'var(--surface-1)',
        display: 'grid',
        gap: 8,
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <span style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-secondary)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
          Version History
        </span>
        <button
          type="button"
          className="btn btn-ghost btn-sm"
          style={{ fontSize: 11 }}
          onClick={() => setShowVersionUpload(s => !s)}
        >
          {showVersionUpload ? 'Cancel' : 'Upload New Version'}
        </button>
      </div>

      {showVersionUpload && (
        <div
          style={{
            border: `1px dashed ${dragOver ? 'var(--amber)' : 'var(--border-bright)'}`,
            borderRadius: 6,
            padding: 10,
            background: dragOver ? 'var(--amber-dim)' : 'var(--surface-2)',
          }}
          onDragOver={e => { e.preventDefault(); setDragOver(true) }}
          onDragLeave={() => setDragOver(false)}
          onDrop={e => {
            e.preventDefault()
            setDragOver(false)
            const file = e.dataTransfer.files[0]
            if (file) setVersionFile(file)
          }}
        >
          <form onSubmit={handleVersionUpload}>
            <div style={{ display: 'grid', gap: 6 }}>
              <input
                type="file"
                className="form-input"
                style={{ cursor: 'pointer', fontSize: 12 }}
                onChange={e => { const f = e.target.files?.[0]; if (f) setVersionFile(f) }}
                required={!versionFile}
              />
              {versionFile && (
                <div style={{ fontSize: 11, color: 'var(--text-secondary)' }}>
                  {versionFile.name} ({fmtSize(versionFile.size)})
                </div>
              )}
              <input
                className="form-input"
                type="text"
                placeholder="Version note (optional)"
                value={versionNote}
                onChange={e => setVersionNote(e.target.value)}
                style={{ fontSize: 12 }}
              />
              <div style={{ display: 'flex', gap: 6 }}>
                <button
                  type="submit"
                  className="btn btn-secondary"
                  disabled={addVersionMutation.isPending || !versionFile}
                  style={{ fontSize: 12 }}
                >
                  {addVersionMutation.isPending ? 'Uploading…' : 'Upload'}
                </button>
              </div>
            </div>
          </form>
        </div>
      )}

      {isLoading ? (
        <div style={{ fontSize: 12, color: 'var(--text-dim)' }}>Loading…</div>
      ) : !doc?.versions?.length ? (
        <div style={{ fontSize: 12, color: 'var(--text-dim)' }}>No version history available.</div>
      ) : (
        <div style={{ display: 'grid', gap: 4 }}>
          {[...(doc.versions ?? [])].reverse().map(v => (
            <div
              key={v.versionId}
              style={{
                display: 'grid',
                gridTemplateColumns: '1fr auto',
                gap: 8,
                padding: '6px 8px',
                borderRadius: 4,
                background: v.versionId === doc.currentVersionId ? 'var(--amber-dim)' : 'var(--surface-2)',
                border: `1px solid ${v.versionId === doc.currentVersionId ? 'var(--amber)' : 'var(--border)'}`,
                fontSize: 12,
              }}
            >
              <div style={{ display: 'grid', gap: 2 }}>
                <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                  <span style={{ fontFamily: 'var(--font-mono)', fontWeight: 600 }}>v{v.versionNumber}</span>
                  <span style={{ color: 'var(--text-secondary)', fontFamily: 'var(--font-mono)' }}>{v.fileName}</span>
                  {v.versionId === doc.currentVersionId && (
                    <span style={{ fontSize: 10, color: 'var(--amber-bright)', textTransform: 'uppercase', letterSpacing: '0.04em' }}>current</span>
                  )}
                </div>
                <div style={{ display: 'flex', gap: 8, color: 'var(--text-dim)', fontSize: 11 }}>
                  <span>{fmtSize(v.sizeBytes ?? 0)}</span>
                  <span>{v.uploadedAt ? fmtDate(v.uploadedAt) : ''}</span>
                </div>
              </div>
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                style={{ fontSize: 11 }}
                onClick={() => window.open(`/api/v1/documents/${documentId}/download`, '_blank')}
                title="Download this version"
              >
                DL
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
