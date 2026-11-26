import { useMutation, useQuery, useQueryClient, type QueryClient } from '@tanstack/react-query'
import { RemindersApi, type SetReminderRequest } from '../api'
import { apiConfig } from '../lib/apiConfig'

const api = new RemindersApi(apiConfig)

const invalidateReminders = (qc: QueryClient) => {
  void qc.invalidateQueries({ queryKey: ['reminders'] })
  void qc.invalidateQueries({ queryKey: ['today'] })
}

/** The trust's reminders: showing on Today, upcoming, and done in the last 30 days. */
export function useReminders() {
  return useQuery({
    queryKey: ['reminders'],
    queryFn: () => api.listReminders(),
  })
}

/** Sets a reminder for the trust. */
export function useSetReminder() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (request: SetReminderRequest) => api.setReminder({ setReminderRequest: request }),
    onSuccess: () => invalidateReminders(qc),
  })
}

/** Marks a reminder done, so it leaves Today. */
export function useMarkReminderDone() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (reminderId: string) => api.markReminderDone({ reminderId }),
    onSuccess: () => invalidateReminders(qc),
  })
}
