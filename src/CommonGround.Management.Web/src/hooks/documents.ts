import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { DocumentsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import { getCsrfToken } from '../lib/csrf'
import type {
  AttachDocumentRequest,
  TagDocumentRequest,
  UpdateDocumentMetadataRequest,
} from '../api'

const api = new DocumentsApi(apiConfig)

// ── Keys ────────────────────────────────────────────
const KEYS = {
  doc: (id: string) => ['document', id] as const,
  entityDocs: (entityType: string, entityId: string) => ['entity-documents', entityType, entityId] as const,
  list: (filters?: Record<string, unknown>) => ['documents', filters ?? {}] as const,
} as const

// ── Queries ──────────────────────────────────────────
export function useDocument(id: string | null) {
  return useQuery({
    queryKey: KEYS.doc(id ?? ''),
    queryFn: () => api.getDocument({ documentId: id! }),
    enabled: !!id,
  })
}

export function useEntityDocuments(
  entityType: string | null,
  entityId: string | null,
  page = 1,
  pageSize = 20,
) {
  return useQuery({
    queryKey: [...KEYS.entityDocs(entityType ?? '', entityId ?? ''), page, pageSize],
    queryFn: () => api.getEntityDocuments({ entityType: entityType!, entityId: entityId!, pageNumber: page, pageSize }),
    enabled: !!entityType && !!entityId,
  })
}

export function useDocuments(params: {
  page?: number
  pageSize?: number
  includeDeleted?: boolean
  contentType?: string
  tag?: string
  search?: string
  /** ISO date (YYYY-MM-DD); only documents expiring before this date are returned, soonest first. */
  expiringBefore?: string
} = {}) {
  const { page = 1, pageSize = 20, includeDeleted, contentType, tag, search, expiringBefore } = params
  return useQuery({
    queryKey: KEYS.list({ page, pageSize, includeDeleted, contentType, tag, search, expiringBefore }),
    queryFn: () => api.listDocuments({
      pageNumber: page,
      pageSize,
      includeDeleted,
      contentType,
      tag,
      search,
      expiringBefore: expiringBefore ? new Date(expiringBefore) : undefined,
    }),
  })
}

/**
 * Documents expiring within the next N days, including already-expired ones, soonest first.
 * The cutoff is a whole date so the query key stays stable across renders.
 */
export function useExpiringDocuments(daysAhead: number, page = 1) {
  const cutoff = new Date()
  cutoff.setUTCDate(cutoff.getUTCDate() + daysAhead + 1)
  return useDocuments({ expiringBefore: cutoff.toISOString().slice(0, 10), page })
}

// ── Mutations ────────────────────────────────────────

/** Multipart upload — the generated client can't handle this, so we use fetch directly. */
export function useUploadDocument() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({
      metadata,
      file,
    }: {
      metadata: {
        title: string
        sensitivity: string
        retention: string
        initialAttachment?: { entityType: string; entityId: string; role?: string } | null
      }
      file: File
    }) => {
      const form = new FormData()
      form.append('metadata', JSON.stringify(metadata))
      form.append('file', file, file.name)

      const csrf = getCsrfToken() ?? ''

      const res = await fetch('/api/v1/documents', {
        method: 'POST',
        body: form,
        credentials: 'include',
        headers: csrf ? { 'X-CSRF-Token': csrf } : {},
      })
      if (!res.ok) throw new Error(`Upload failed: ${res.statusText}`)
      return res.json() as Promise<{ documentId: string }>
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['documents'] })
      qc.invalidateQueries({ queryKey: ['entity-documents'] })
    },
  })
}

export function useAddDocumentVersion() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({
      documentId,
      file,
      note,
    }: {
      documentId: string
      file: File
      note?: string
    }) => {
      const form = new FormData()
      form.append('metadata', JSON.stringify({ note: note ?? null }))
      form.append('file', file, file.name)

      const csrf = getCsrfToken() ?? ''

      const res = await fetch(`/api/v1/documents/${documentId}/versions`, {
        method: 'POST',
        body: form,
        credentials: 'include',
        headers: csrf ? { 'X-CSRF-Token': csrf } : {},
      })
      if (!res.ok) throw new Error(`Version upload failed: ${res.statusText}`)
      return res.json() as Promise<{ documentId: string; versionId: string }>
    },
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: KEYS.doc(v.documentId) })
      qc.invalidateQueries({ queryKey: ['entity-documents'] })
    },
  })
}

export function useAttachDocument() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ documentId, req }: { documentId: string; req: AttachDocumentRequest }) =>
      api.attachDocument({ documentId, attachDocumentRequest: req }),
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: KEYS.doc(v.documentId) })
      qc.invalidateQueries({ queryKey: ['entity-documents'] })
    },
  })
}

export function useSoftDeleteDocument() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ documentId, reason }: { documentId: string; reason: string }) =>
      api.softDeleteDocument({ documentId, softDeleteDocumentRequest: { reason } }),
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: KEYS.doc(v.documentId) })
      qc.invalidateQueries({ queryKey: ['documents'] })
      qc.invalidateQueries({ queryKey: ['entity-documents'] })
    },
  })
}

export function useUpdateDocumentMetadata() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ documentId, req }: { documentId: string; req: UpdateDocumentMetadataRequest }) =>
      api.updateDocumentMetadata({ documentId, updateDocumentMetadataRequest: req }),
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: KEYS.doc(v.documentId) })
      qc.invalidateQueries({ queryKey: ['documents'] })
      qc.invalidateQueries({ queryKey: ['entity-documents'] })
    },
  })
}

export function useTagDocument() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ documentId, req }: { documentId: string; req: TagDocumentRequest }) =>
      api.tagDocument({ documentId, tagDocumentRequest: req }),
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: KEYS.doc(v.documentId) })
      qc.invalidateQueries({ queryKey: ['documents'] })
    },
  })
}

export function useUntagDocument() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ documentId, tag }: { documentId: string; tag: string }) =>
      api.untagDocument({ documentId, tag }),
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: KEYS.doc(v.documentId) })
      qc.invalidateQueries({ queryKey: ['documents'] })
    },
  })
}

/** Downloads a document (its current version, or the given one) — opens the presigned URL in a new tab. */
export function downloadDocument(documentId: string, versionId?: string) {
  const query = versionId ? `?versionId=${encodeURIComponent(versionId)}` : ''
  window.open(`/api/v1/documents/${documentId}/download${query}`, '_blank')
}
