import { useMutation, useQueryClient } from '@tanstack/react-query'
import { PortfolioImportApi, PortfolioImportResponseFromJSON, type PortfolioImportResponse } from '../api'
import { ResponseError } from '../api/runtime'
import { apiConfig } from '../lib/apiConfig'

const api = new PortfolioImportApi(apiConfig)

export function useCheckPortfolioImport() {
  return useMutation({
    mutationFn: (csv: string) => api.checkPortfolioImport({ portfolioImportRequest: { csv } }),
  })
}

/**
 * Imports a file. A 422 is not thrown: its body is the check's result, with the row errors or the row the
 * import stopped at.
 */
export function useImportPortfolio() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (csv: string): Promise<PortfolioImportResponse> => {
      try {
        return await api.importPortfolio({ portfolioImportRequest: { csv } })
      } catch (err) {
        if (err instanceof ResponseError && err.response.status === 422) {
          return PortfolioImportResponseFromJSON(await err.response.json())
        }
        throw err
      }
    },
    // Even a stopped import created rows before the one it stopped at.
    onSettled: () => { void qc.invalidateQueries() },
  })
}
