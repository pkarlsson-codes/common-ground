import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { StewardshipApi } from '../api'
import { apiConfig } from '../lib/apiConfig'

const api = new StewardshipApi(apiConfig)

// Every stewardship query key starts with 'stewardship', so any action refreshes both queues and the open case.
const STEWARDSHIP_KEY = ['stewardship'] as const

function useStewardshipMutation<T>(mutationFn: (variables: T) => Promise<unknown>) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn,
    onSuccess: () => qc.invalidateQueries({ queryKey: STEWARDSHIP_KEY }),
  })
}

// ── Queues ────────────────────────────────────────────────────────────────────

/** Every open stewardship case, with its lease's homeowner and address. */
export function useStewardshipQueue() {
  return useQuery({
    queryKey: [...STEWARDSHIP_KEY, 'queue', 'cases'],
    queryFn: () => api.getStewardshipQueue(),
  })
}

/** Every unresolved foreclosure prevention case (newest first), with its lease's homeowner and address. */
export function useForeclosurePreventionQueue() {
  return useQuery({
    queryKey: [...STEWARDSHIP_KEY, 'queue', 'fpc'],
    queryFn: () => api.getForeclosurePreventionQueue(),
  })
}

// ── Single cases ──────────────────────────────────────────────────────────────

export function useStewardshipCase(caseId: string | null) {
  return useQuery({
    queryKey: [...STEWARDSHIP_KEY, 'case', caseId],
    queryFn: () => api.getStewardshipCase({ caseId: caseId! }),
    enabled: !!caseId,
  })
}

export function useForeclosurePreventionCase(caseId: string | null) {
  return useQuery({
    queryKey: [...STEWARDSHIP_KEY, 'fpc', caseId],
    queryFn: () => api.getForeclosurePreventionCase({ caseId: caseId! }),
    enabled: !!caseId,
  })
}

// ── Occupancy verification mutations ─────────────────────────────────────────

export function useInitiateOccupancyVerification() {
  return useStewardshipMutation(({ caseId, periodYear, surveyDeadline }: {
    caseId: string
    periodYear: number
    surveyDeadline?: string
  }) =>
    api.initiateOccupancyVerification({
      caseId,
      initiateOccupancyVerificationRequest: {
        periodYear,
        surveyDeadline: surveyDeadline ? new Date(surveyDeadline) : undefined,
      },
    }))
}

export function useSubmitOccupancyVerification() {
  return useStewardshipMutation(({ caseId, verificationId, isPrimaryResident, subletDisclosed, majorChanges, submittedDate }: {
    caseId: string
    verificationId: string
    isPrimaryResident: boolean
    subletDisclosed: boolean
    majorChanges?: string
    submittedDate: string
  }) =>
    api.submitOccupancyVerification({
      caseId,
      verificationId,
      submitOccupancyVerificationRequest: {
        isPrimaryResident,
        subletDisclosed,
        majorChanges,
        submittedDate: new Date(submittedDate),
      },
    }))
}

export function useMarkOccupancyVerificationOverdue() {
  return useStewardshipMutation(({ caseId, verificationId, deadline }: {
    caseId: string
    verificationId: string
    deadline: string
  }) =>
    api.markOccupancyVerificationOverdue({
      caseId,
      verificationId,
      markOccupancyVerificationOverdueRequest: { deadline: new Date(deadline) },
    }))
}

export function useVerifyOccupancy() {
  return useStewardshipMutation(({ caseId, verificationId, verifiedDate }: {
    caseId: string
    verificationId: string
    verifiedDate: string
  }) =>
    api.verifyOccupancy({
      caseId,
      verificationId,
      verifyOccupancyRequest: { verifiedDate: new Date(verifiedDate) },
    }))
}

export function useFailOccupancy() {
  return useStewardshipMutation(({ caseId, verificationId, reason, failedDate }: {
    caseId: string
    verificationId: string
    reason: string
    failedDate: string
  }) =>
    api.failOccupancy({
      caseId,
      verificationId,
      failOccupancyRequest: { reason, failedDate: new Date(failedDate) },
    }))
}

// ── Property inspection mutations ─────────────────────────────────────────────

export function useSchedulePropertyInspection() {
  return useStewardshipMutation(({ caseId, scheduledDate, inspectorName }: {
    caseId: string
    scheduledDate: string
    inspectorName: string
  }) =>
    api.schedulePropertyInspection({
      caseId,
      schedulePropertyInspectionRequest: { scheduledDate: new Date(scheduledDate), inspectorName },
    }))
}

export function useCompletePropertyInspection() {
  return useStewardshipMutation(({ caseId, inspectionId, occupancyConfirmed, conditionNotes, completedDate }: {
    caseId: string
    inspectionId: string
    occupancyConfirmed: boolean
    conditionNotes?: string
    completedDate: string
  }) =>
    api.completePropertyInspection({
      caseId,
      inspectionId,
      completePropertyInspectionRequest: {
        occupancyConfirmed,
        conditionNotes,
        completedDate: new Date(completedDate),
      },
    }))
}

export function useCancelPropertyInspection() {
  return useStewardshipMutation(({ caseId, inspectionId, reason }: {
    caseId: string
    inspectionId: string
    reason: string
  }) =>
    api.cancelPropertyInspection({
      caseId,
      inspectionId,
      cancelPropertyInspectionRequest: { reason },
    }))
}

// ── Lease violation mutations ─────────────────────────────────────────────────

export function useIdentifyViolation() {
  return useStewardshipMutation(({ caseId, violationType, description, identifiedDate }: {
    caseId: string
    violationType: string
    description: string
    identifiedDate: string
  }) =>
    api.identifyViolation({
      caseId,
      identifyViolationRequest: { violationType, description, identifiedDate: new Date(identifiedDate) },
    }))
}

export function useIssueViolationNotice() {
  return useStewardshipMutation(({ caseId, violationId, noticeDate, cureDeadline }: {
    caseId: string
    violationId: string
    noticeDate: string
    cureDeadline: string
  }) =>
    api.issueViolationNotice({
      caseId,
      violationId,
      issueViolationNoticeRequest: { noticeDate: new Date(noticeDate), cureDeadline: new Date(cureDeadline) },
    }))
}

export function useCureViolation() {
  return useStewardshipMutation(({ caseId, violationId, curedDate }: {
    caseId: string
    violationId: string
    curedDate: string
  }) =>
    api.cureViolation({
      caseId,
      violationId,
      cureViolationRequest: { curedDate: new Date(curedDate) },
    }))
}

export function useEscalateViolation() {
  return useStewardshipMutation(({ caseId, violationId, reason, escalatedDate }: {
    caseId: string
    violationId: string
    reason: string
    escalatedDate: string
  }) =>
    api.escalateViolation({
      caseId,
      violationId,
      escalateViolationRequest: { reason, escalatedDate: new Date(escalatedDate) },
    }))
}

// ── Foreclosure prevention mutations ──────────────────────────────────────────

export function useOpenForeclosurePreventionCase() {
  return useStewardshipMutation((body: {
    groundLeaseId: string
    servicerName: string
    delinquentMonths: number
    recordedDate: string
  }) =>
    api.openForeclosurePreventionCase({
      openForeclosurePreventionCaseRequest: { ...body, recordedDate: new Date(body.recordedDate) },
    }))
}

export function useRecordServicerDelinquency() {
  return useStewardshipMutation(({ caseId, ...body }: {
    caseId: string
    servicerName: string
    delinquentMonths: number
    recordedDate: string
  }) =>
    api.recordServicerDelinquency({
      caseId,
      recordServicerDelinquencyRequest: { ...body, recordedDate: new Date(body.recordedDate) },
    }))
}

export function useMakeCounselingReferral() {
  return useStewardshipMutation(({ caseId, agencyName, referralDate }: {
    caseId: string
    agencyName: string
    referralDate: string
  }) =>
    api.makeCounselingReferral({
      caseId,
      makeCounselingReferralRequest: { agencyName, referralDate: new Date(referralDate) },
    }))
}

export function useRecordCounselingOutcome() {
  return useStewardshipMutation(({ caseId, referralId, outcome, outcomeDate }: {
    caseId: string
    referralId: string
    outcome: string
    outcomeDate: string
  }) =>
    api.recordCounselingOutcome({
      caseId,
      referralId,
      recordCounselingOutcomeRequest: { outcome, outcomeDate: new Date(outcomeDate) },
    }))
}

export function useRequestForbearance() {
  return useStewardshipMutation(({ caseId, lenderName, requestedDate }: {
    caseId: string
    lenderName: string
    requestedDate: string
  }) =>
    api.requestForbearance({
      caseId,
      requestForbearanceRequest: { lenderName, requestedDate: new Date(requestedDate) },
    }))
}

export function useRecordForbearanceOutcome() {
  return useStewardshipMutation(({ caseId, requestId, outcome, outcomeDate, notes }: {
    caseId: string
    requestId: string
    outcome: string
    outcomeDate: string
    notes?: string
  }) =>
    api.recordForbearanceOutcome({
      caseId,
      requestId,
      recordForbearanceOutcomeRequest: { outcome, outcomeDate: new Date(outcomeDate), notes },
    }))
}

export function useActivateCurePeriod() {
  return useStewardshipMutation(({ caseId, lraId, activationDate, curePeriodDays }: {
    caseId: string
    lraId: string
    activationDate: string
    curePeriodDays: number
  }) =>
    api.activateCurePeriod({
      caseId,
      activateCurePeriodRequest: { lraId, activationDate: new Date(activationDate), curePeriodDays },
    }))
}

export function useResolveForeclosurePreventionCase() {
  return useStewardshipMutation(({ caseId, outcome, resolvedDate }: {
    caseId: string
    outcome: string
    resolvedDate: string
  }) =>
    api.resolveForeclosurePreventionCase({
      caseId,
      resolveForeclosurePreventionCaseRequest: { outcome, resolvedDate: new Date(resolvedDate) },
    }))
}
