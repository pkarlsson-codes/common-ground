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
