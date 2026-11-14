/** WaitingListRemovalReason values the API accepts, with the label and hint staff see. */
export const WAITING_LIST_REMOVAL_REASONS = [
  { value: 'NoLongerEligible', label: 'No longer eligible', hint: 'Income, household size or other eligibility changed' },
  { value: 'NoResponse', label: 'No response', hint: "The trust couldn't reach the household" },
  { value: 'FalseInformation', label: 'False information', hint: 'What the household declared turned out to be false' },
  { value: 'Other', label: 'Other', hint: 'Needs a note' },
] as const

export const waitingListRemovalReasonLabel = (value?: string | null) =>
  WAITING_LIST_REMOVAL_REASONS.find(r => r.value === value)?.label ?? value ?? '—'
