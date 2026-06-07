import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { StewardshipApi } from '../api'
import { apiConfig } from '../lib/apiConfig'

const api = new StewardshipApi(apiConfig)

// ── Query keys ────────────────────────────────────────────────────────────────

const CASE_KEY = (groundLeaseId: string) => ['stewardship-case', groundLeaseId] as const
const FPC_KEY = (groundLeaseId: string) => ['foreclosure-prevention-cases', groundLeaseId] as const

// ── Stewardship case queries ──────────────────────────────────────────────────

/** Fetches all stewardship cases for a ground lease. Returns empty array while id is null. */
export function useStewardshipCase(groundLeaseId: string | null) {
  return useQuery({
    queryKey: CASE_KEY(groundLeaseId ?? ''),
    queryFn: () => api.getStewardshipCases({ groundLeaseId: groundLeaseId! }),
    enabled: !!groundLeaseId,
    select: (response) => response.items?.[0] ?? null,   // one case per lease by domain invariant
  })
}

// ── Occupancy verification mutations ─────────────────────────────────────────

export function useInitiateOccupancyVerification(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      periodYear,
      surveyDeadline,
    }: {
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
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: CASE_KEY(groundLeaseId) }),
  })
}

export function useSubmitOccupancyVerification(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      verificationId,
      isPrimaryResident,
      subletDisclosed,
      majorChanges,
      submittedDate,
    }: {
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
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: CASE_KEY(groundLeaseId) }),
  })
}

export function useMarkOccupancyVerificationOverdue(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      verificationId,
      deadline,
    }: {
      caseId: string
      verificationId: string
      deadline: string
    }) =>
      api.markOccupancyVerificationOverdue({
        caseId,
        verificationId,
        markOccupancyVerificationOverdueRequest: { deadline: new Date(deadline) },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: CASE_KEY(groundLeaseId) }),
  })
}

export function useVerifyOccupancy(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      verificationId,
      verifiedDate,
    }: {
      caseId: string
      verificationId: string
      verifiedDate: string
    }) =>
      api.verifyOccupancy({
        caseId,
        verificationId,
        verifyOccupancyRequest: { verifiedDate: new Date(verifiedDate) },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: CASE_KEY(groundLeaseId) }),
  })
}

export function useFailOccupancy(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      verificationId,
      reason,
      failedDate,
    }: {
      caseId: string
      verificationId: string
      reason: string
      failedDate: string
    }) =>
      api.failOccupancy({
        caseId,
        verificationId,
        failOccupancyRequest: { reason, failedDate: new Date(failedDate) },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: CASE_KEY(groundLeaseId) }),
  })
}

// ── Property inspection mutations ─────────────────────────────────────────────

export function useSchedulePropertyInspection(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      scheduledDate,
      inspectorName,
    }: {
      caseId: string
      scheduledDate: string
      inspectorName: string
    }) =>
      api.schedulePropertyInspection({
        caseId,
        schedulePropertyInspectionRequest: { scheduledDate: new Date(scheduledDate), inspectorName },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: CASE_KEY(groundLeaseId) }),
  })
}

export function useCompletePropertyInspection(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      inspectionId,
      occupancyConfirmed,
      conditionNotes,
      completedDate,
    }: {
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
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: CASE_KEY(groundLeaseId) }),
  })
}

export function useCancelPropertyInspection(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      inspectionId,
      reason,
    }: {
      caseId: string
      inspectionId: string
      reason: string
    }) =>
      api.cancelPropertyInspection({
        caseId,
        inspectionId,
        cancelPropertyInspectionRequest: { reason },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: CASE_KEY(groundLeaseId) }),
  })
}

// ── Lease violation mutations ─────────────────────────────────────────────────

export function useIdentifyViolation(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      violationType,
      description,
      identifiedDate,
    }: {
      caseId: string
      violationType: string
      description: string
      identifiedDate: string
    }) =>
      api.identifyViolation({
        caseId,
        identifyViolationRequest: { violationType, description, identifiedDate: new Date(identifiedDate) },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: CASE_KEY(groundLeaseId) }),
  })
}

export function useIssueViolationNotice(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      violationId,
      noticeDate,
      cureDeadline,
    }: {
      caseId: string
      violationId: string
      noticeDate: string
      cureDeadline: string
    }) =>
      api.issueViolationNotice({
        caseId,
        violationId,
        issueViolationNoticeRequest: { noticeDate: new Date(noticeDate), cureDeadline: new Date(cureDeadline) },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: CASE_KEY(groundLeaseId) }),
  })
}

export function useCureViolation(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      violationId,
      curedDate,
    }: {
      caseId: string
      violationId: string
      curedDate: string
    }) =>
      api.cureViolation({
        caseId,
        violationId,
        cureViolationRequest: { curedDate: new Date(curedDate) },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: CASE_KEY(groundLeaseId) }),
  })
}

export function useEscalateViolation(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      violationId,
      reason,
      escalatedDate,
    }: {
      caseId: string
      violationId: string
      reason: string
      escalatedDate: string
    }) =>
      api.escalateViolation({
        caseId,
        violationId,
        escalateViolationRequest: { reason, escalatedDate: new Date(escalatedDate) },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: CASE_KEY(groundLeaseId) }),
  })
}

// ── Foreclosure prevention queries and mutations ──────────────────────────────

export function useForeclosurePreventionCases(groundLeaseId: string | null, page = 1) {
  return useQuery({
    queryKey: [...FPC_KEY(groundLeaseId ?? ''), page],
    queryFn: () =>
      api.getForeclosurePreventionCases({ groundLeaseId: groundLeaseId!, pageNumber: page }),
    enabled: !!groundLeaseId,
  })
}

export function useOpenForeclosurePreventionCase(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: {
      servicerName: string
      delinquentMonths: number
      recordedDate: string
    }) =>
      api.openForeclosurePreventionCase({
        openForeclosurePreventionCaseRequest: {
          groundLeaseId,
          ...body,
          recordedDate: new Date(body.recordedDate),
        },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: FPC_KEY(groundLeaseId) }),
  })
}

export function useRecordServicerDelinquency(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      ...body
    }: {
      caseId: string
      servicerName: string
      delinquentMonths: number
      recordedDate: string
    }) =>
      api.recordServicerDelinquency({
        caseId,
        recordServicerDelinquencyRequest: { ...body, recordedDate: new Date(body.recordedDate) },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: FPC_KEY(groundLeaseId) }),
  })
}

export function useMakeCounselingReferral(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      agencyName,
      referralDate,
    }: {
      caseId: string
      agencyName: string
      referralDate: string
    }) =>
      api.makeCounselingReferral({
        caseId,
        makeCounselingReferralRequest: { agencyName, referralDate: new Date(referralDate) },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: FPC_KEY(groundLeaseId) }),
  })
}

export function useRecordCounselingOutcome(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      referralId,
      outcome,
      outcomeDate,
    }: {
      caseId: string
      referralId: string
      outcome: string
      outcomeDate: string
    }) =>
      api.recordCounselingOutcome({
        caseId,
        referralId,
        recordCounselingOutcomeRequest: { outcome, outcomeDate: new Date(outcomeDate) },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: FPC_KEY(groundLeaseId) }),
  })
}

export function useRequestForbearance(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      lenderName,
      requestedDate,
    }: {
      caseId: string
      lenderName: string
      requestedDate: string
    }) =>
      api.requestForbearance({
        caseId,
        requestForbearanceRequest: { lenderName, requestedDate: new Date(requestedDate) },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: FPC_KEY(groundLeaseId) }),
  })
}

export function useRecordForbearanceOutcome(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      requestId,
      outcome,
      outcomeDate,
      notes,
    }: {
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
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: FPC_KEY(groundLeaseId) }),
  })
}

export function useActivateCurePeriod(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      lraId,
      activationDate,
      curePeriodDays,
    }: {
      caseId: string
      lraId: string
      activationDate: string
      curePeriodDays: number
    }) =>
      api.activateCurePeriod({
        caseId,
        activateCurePeriodRequest: {
          lraId,
          activationDate: new Date(activationDate),
          curePeriodDays,
        },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: FPC_KEY(groundLeaseId) }),
  })
}

export function useResolveForeclosurePreventionCase(groundLeaseId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      caseId,
      outcome,
      resolvedDate,
    }: {
      caseId: string
      outcome: string
      resolvedDate: string
    }) =>
      api.resolveForeclosurePreventionCase({
        caseId,
        resolveForeclosurePreventionCaseRequest: { outcome, resolvedDate: new Date(resolvedDate) },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: FPC_KEY(groundLeaseId) }),
  })
}

// ── Slice 2: list-all queries for the queue page ──────────────────────────────

export function useAllStewardshipCases(page = 1) {
  return useQuery({
    queryKey: ['stewardship', 'cases', 'all', page],
    queryFn: () => api.getStewardshipCases({ pageNumber: page }),
  })
}

export function useAllForeclosurePreventionCases(page = 1) {
  return useQuery({
    queryKey: ['stewardship', 'fpc', 'all', page],
    queryFn: () => api.getForeclosurePreventionCases({ pageNumber: page }),
  })
}

/** Opens an FPC case for any ground lease (used from the global queue page). */
export function useOpenFpcCase() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: {
      groundLeaseId: string
      servicerName: string
      delinquentMonths: number
      recordedDate: string
    }) =>
      api.openForeclosurePreventionCase({
        openForeclosurePreventionCaseRequest: { ...body, recordedDate: new Date(body.recordedDate) },
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['stewardship', 'fpc', 'all'] })
    },
  })
}
