import { useState } from 'react'
import type { FormEvent } from 'react'
import type { DocumentResponse } from '../../api'
import SlideOver from '../SlideOver'
import NavIdLink from '../NavIdLink'
import DocumentSensitivityBadge from '../DocumentSensitivityBadge'
import {
  useDocument,
  useUpdateDocumentMetadata,
  useTagDocument,
  useUntagDocument,
  downloadDocument,
} from '../../hooks/documents'
import { useToast } from '../../hooks/useToast'
import { getErrorMessage } from '../../lib/getErrorMessage'
import { fmtDate, fmtExpiry, fmtSize, isExpired, isExpiringSoon } from './format'
import { DateInput } from '../DateInput'

const SENSITIVITY_OPTIONS = ['Public', 'Internal', 'Confidential', 'Restricted']

// Where an attachment's entity type has a page, a link opens that entity's panel.
const ENTITY_LINKS: Record<string, { label: string; to: string }> = {
  Building: { label: 'Open Home', to: '/homes' },
}

interface Props {
  documentId: string
  onClose: () => void
}

export default function DocumentDetailPanel({ documentId, onClose }: Props) {
  const { data: doc, isLoading, isError } = useDocument(documentId)

  return (
    <SlideOver
      title={doc?.title ?? 'Document'}
      subtitle={doc?.currentFileName}
      onClose={onClose}
    >
      {isLoading && <div className="skeleton-rows"><div className="skeleton-row" /><div className="skeleton-row" /></div>}
      {isError && <div className="form-error">Could not load this document.</div>}
      {doc && (
        <>
          <div className="detail-section">
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              onClick={() => downloadDocument(doc.id!)}
            >
              Download
            </button>
          </div>
          {/* Keyed so the forms reset to the saved values when the document changes. */}
          <DetailsSection key={`${doc.title}|${doc.sensitivity}|${doc.expiresOn ?? ''}`} doc={doc} />
          <TagsSection doc={doc} />
          <VersionsSection doc={doc} />
          <AttachmentsSection doc={doc} />
        </>
      )}
    </SlideOver>
  )
}

function DetailsSection({ doc }: { doc: DocumentResponse }) {
  const { success, error: showError } = useToast()
  const update = useUpdateDocumentMetadata()
  const [form, setForm] = useState({
    title: doc.title ?? '',
    sensitivity: doc.sensitivity ?? 'Internal',
    expiresOn: doc.expiresOn ?? null,
  })

  const savedExpiresOn = doc.expiresOn ?? null
  const dirty = form.title.trim() !== doc.title
    || form.sensitivity !== doc.sensitivity
    || form.expiresOn !== savedExpiresOn

  const handleSave = async (e: FormEvent) => {
    e.preventDefault()
    try {
      await update.mutateAsync({
        documentId: doc.id!,
        req: {
          title: form.title.trim(),
          sensitivity: form.sensitivity,
          expiresOn: form.expiresOn,
          clearExpiresOn: !form.expiresOn,
        },
      })
      success('Document updated')
    } catch (err) {
      showError(await getErrorMessage(err, 'Could not update the document.'))
    }
  }

  return (
    <div className="detail-section">
      <div className="detail-section__title">Details</div>
      <div style={{ display: 'flex', gap: 8, alignItems: 'center', marginBottom: 12, flexWrap: 'wrap', fontSize: 12 }}>
        {doc.sensitivity && <DocumentSensitivityBadge sensitivity={doc.sensitivity} />}
        {doc.expiresOn && (
          <span style={{ color: isExpiringSoon(doc.expiresOn) ? 'var(--amber-bright)' : 'var(--text-secondary)' }}>
            {isExpired(doc.expiresOn) ? 'Expired' : 'Expires'} {fmtExpiry(doc.expiresOn)}
          </span>
        )}
      </div>
      <form onSubmit={handleSave}>
        <div className="form-grid" style={{ marginBottom: 12 }}>
          <div className="form-field full">
            <label className="form-label">Title</label>
            <input
              className="form-input"
              value={form.title}
              onChange={e => setForm(f => ({ ...f, title: e.target.value }))}
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
              {SENSITIVITY_OPTIONS.map(s => <option key={s} value={s}>{s}</option>)}
            </select>
          </div>
          <div className="form-field">
            <label className="form-label">Expires On</label>
            <DateInput
              className="form-input"
              value={form.expiresOn}
              onChange={v => setForm(f => ({ ...f, expiresOn: v }))}
            />
          </div>
        </div>
        <button type="submit" className="btn btn-secondary" disabled={!dirty || update.isPending}>
          {update.isPending ? 'Saving…' : 'Save Details'}
        </button>
      </form>
      <div style={{ marginTop: 12, fontSize: 12, color: 'var(--text-dim)' }}>
        Retention: {doc.retention} · Uploaded {fmtDate(doc.uploadedAt)}
      </div>
    </div>
  )
}

function TagsSection({ doc }: { doc: DocumentResponse }) {
  const { error: showError } = useToast()
  const tag = useTagDocument()
  const untag = useUntagDocument()
  const [newTag, setNewTag] = useState('')
  const tags = doc.tags ?? []

  const handleAdd = async (e: FormEvent) => {
    e.preventDefault()
    try {
      await tag.mutateAsync({ documentId: doc.id!, req: { tag: newTag.trim() } })
      setNewTag('')
    } catch (err) {
      showError(await getErrorMessage(err, 'Could not add the tag.'))
    }
  }

  const handleRemove = async (t: string) => {
    try {
      await untag.mutateAsync({ documentId: doc.id!, tag: t })
    } catch (err) {
      showError(await getErrorMessage(err, 'Could not remove the tag.'))
    }
  }

  return (
    <div className="detail-section">
      <div className="detail-section__title">Tags</div>
      {tags.length === 0 ? (
        <div style={{ color: 'var(--text-dim)', fontSize: 13, marginBottom: 10 }}>No tags.</div>
      ) : (
        <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap', marginBottom: 10 }}>
          {tags.map(t => (
            <span
              key={t}
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: 4,
                padding: '2px 4px 2px 8px',
                background: 'var(--surface-3)',
                border: '1px solid var(--border)',
                borderRadius: 4,
                fontSize: 12,
                fontFamily: 'var(--font-mono)',
                color: 'var(--text-secondary)',
              }}
            >
              #{t}
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                style={{ padding: '0 4px', lineHeight: 1 }}
                aria-label={`Remove tag ${t}`}
                disabled={untag.isPending}
                onClick={() => handleRemove(t)}
              >
                ×
              </button>
            </span>
          ))}
        </div>
      )}
      <form onSubmit={handleAdd} style={{ display: 'flex', gap: 6 }}>
        <input
          className="form-input"
          style={{ maxWidth: 220 }}
          placeholder="New tag"
          aria-label="New tag"
          value={newTag}
          onChange={e => setNewTag(e.target.value)}
        />
        <button type="submit" className="btn btn-ghost btn-sm" disabled={!newTag.trim() || tag.isPending}>
          Add Tag
        </button>
      </form>
    </div>
  )
}

function VersionsSection({ doc }: { doc: DocumentResponse }) {
  const versions = [...(doc.versions ?? [])].sort((a, b) => (b.versionNumber ?? 0) - (a.versionNumber ?? 0))

  return (
    <div className="detail-section">
      <div className="detail-section__title">Versions</div>
      <div style={{ display: 'grid', gap: 6 }}>
        {versions.map(v => (
          <div
            key={v.versionId}
            style={{ display: 'flex', alignItems: 'center', gap: 10, fontSize: 12, color: 'var(--text-secondary)' }}
          >
            <span style={{ fontFamily: 'var(--font-mono)', color: 'var(--text-bright)' }}>v{v.versionNumber}</span>
            <span style={{ fontFamily: 'var(--font-mono)' }}>{v.fileName}</span>
            {v.sizeBytes != null && <span>{fmtSize(v.sizeBytes)}</span>}
            <span>{fmtDate(v.uploadedAt)}</span>
            <button
              type="button"
              className="btn btn-ghost btn-sm"
              style={{ marginLeft: 'auto' }}
              aria-label={`Download version ${v.versionNumber}`}
              onClick={() => downloadDocument(doc.id!, v.versionId)}
            >
              Download
            </button>
          </div>
        ))}
      </div>
    </div>
  )
}

function AttachmentsSection({ doc }: { doc: DocumentResponse }) {
  const attachments = doc.attachments ?? []

  return (
    <div className="detail-section">
      <div className="detail-section__title">Attached To</div>
      {attachments.length === 0 ? (
        <div style={{ color: 'var(--text-dim)', fontSize: 13 }}>Not attached to any record.</div>
      ) : (
        <div style={{ display: 'grid', gap: 6 }}>
          {attachments.map(a => {
            const link = ENTITY_LINKS[a.entityType ?? '']
            return (
              <div key={`${a.entityType}:${a.entityId}:${a.role ?? ''}`} style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                {link
                  ? <NavIdLink id={a.entityId} to={link.to} label={link.label} />
                  : <span style={{ fontSize: 12, fontFamily: 'var(--font-mono)' }}>{a.entityType} {a.entityId}</span>}
                {a.role && <span style={{ fontSize: 12, color: 'var(--text-dim)' }}>{a.role}</span>}
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
