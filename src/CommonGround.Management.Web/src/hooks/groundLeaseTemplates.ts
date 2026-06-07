import { useQuery } from '@tanstack/react-query'
import { GroundLeaseTemplatesApi } from '../api'
import { apiConfig } from '../lib/apiConfig'

const api = new GroundLeaseTemplatesApi(apiConfig)

export function useGroundLeaseTemplates(pageNumber: number, search?: string, status?: string) {
  return useQuery({
    queryKey: ['ground-lease-templates', pageNumber, search, status],
    queryFn: () => api.getGroundLeaseTemplates({ pageNumber, pageSize: 20, search, status }),
  })
}
