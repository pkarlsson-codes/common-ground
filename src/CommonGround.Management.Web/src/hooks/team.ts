import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { TrustInvitationsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'

const api = new TrustInvitationsApi(apiConfig)

export function useTrustMembers() {
  return useQuery({
    queryKey: ['team', 'members'],
    queryFn: () => api.listTrustMembers(),
  })
}

export function usePendingInvitations() {
  return useQuery({
    queryKey: ['team', 'invitations'],
    queryFn: () => api.listTrustInvitations(),
  })
}

export function useCreateInvitation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (email: string) => api.createTrustInvitation({ createTrustInvitationRequest: { email } }),
    onSuccess: () => { void qc.invalidateQueries({ queryKey: ['team', 'invitations'] }) },
  })
}

export function useCancelInvitation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (invitationId: string) => api.cancelTrustInvitation({ invitationId }),
    onSuccess: () => { void qc.invalidateQueries({ queryKey: ['team', 'invitations'] }) },
  })
}

export function useRemoveMember() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (userId: string) => api.removeTrustMember({ userId }),
    onSuccess: () => { void qc.invalidateQueries({ queryKey: ['team', 'members'] }) },
  })
}

/** What an invite link joins. Not retried: an invalid link stays invalid. */
export function useInvitationPreview(token: string) {
  return useQuery({
    queryKey: ['invitation-preview', token],
    queryFn: () => api.previewTrustInvitation({ trustInvitationTokenRequest: { token } }),
    retry: false,
  })
}

export function useAcceptInvitation() {
  return useMutation({
    mutationFn: (token: string) => api.acceptTrustInvitation({ trustInvitationTokenRequest: { token } }),
  })
}

/** The link a colleague opens to join. */
export function inviteLink(token: string): string {
  return `${window.location.origin}/invite/${token}`
}
