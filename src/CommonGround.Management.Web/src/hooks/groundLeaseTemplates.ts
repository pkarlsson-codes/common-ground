import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { GroundLeaseTemplatesApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type { CreateGroundLeaseTemplateForm } from './types'

const api = new GroundLeaseTemplatesApi(apiConfig)

export function useGroundLeaseTemplates(pageNumber: number, search?: string, status?: string) {
  return useQuery({
    queryKey: ['ground-lease-templates', pageNumber, search, status],
    queryFn: () => api.getGroundLeaseTemplates({ pageNumber, pageSize: 20, search, status }),
  })
}

export function useCreateGroundLeaseTemplate() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (form: CreateGroundLeaseTemplateForm) =>
      api.createGroundLeaseTemplate({
        createGroundLeaseTemplateRequest: {
          templateName: form.templateName,
          termYears: form.termYears,
          covenantTerms: form.covenantTerms,
          formulaKind: form.formulaKind,
          appreciationShare: form.appreciationShare ?? undefined,
          indexType: form.indexType ?? undefined,
          baselineIndexValue: form.baselineIndexValue ?? undefined,
          annualPercentage: form.annualPercentage ?? undefined,
          improvementsCap: form.improvementsCap ?? undefined,
        },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['ground-lease-templates'] }),
  })
}

export function useGroundLeaseTemplate(id: string | null) {
  return useQuery({
    queryKey: ['ground-lease-template', id],
    queryFn: () => api.getGroundLeaseTemplate({ id: id! }),
    enabled: !!id,
  })
}

export function useArchiveGroundLeaseTemplate() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.archiveGroundLeaseTemplate({ id }),
    onSuccess: (_data, id) => {
      qc.invalidateQueries({ queryKey: ['ground-lease-templates'] })
      qc.invalidateQueries({ queryKey: ['ground-lease-template', id] })
    },
  })
}

export function useRestoreGroundLeaseTemplate() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.restoreGroundLeaseTemplate({ id }),
    onSuccess: (_data, id) => {
      qc.invalidateQueries({ queryKey: ['ground-lease-templates'] })
      qc.invalidateQueries({ queryKey: ['ground-lease-template', id] })
    },
  })
}
