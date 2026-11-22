import { useTrustMembers } from './team'

/** Shown for a user id that isn't a current member of the trust (removed members, seeded system users). */
export const UNKNOWN_USER_NAME = 'Former member'

/**
 * Relies on the members endpoint returning every member (it is not paged; see ListTrustMembersHandler).
 * Resolves a user id to the member's name, then email. Returns '' while the member list loads,
 * '—' for an empty id and {@link UNKNOWN_USER_NAME} for an id that isn't a member.
 */
export function useUserName(): (userId?: string | null) => string {
  const { data: members, isLoading } = useTrustMembers()
  return userId => {
    if (!userId) return '—'
    const member = members?.items?.find(m => m.userId === userId)
    if (member) return member.name || member.email || UNKNOWN_USER_NAME
    return isLoading ? '' : UNKNOWN_USER_NAME
  }
}
