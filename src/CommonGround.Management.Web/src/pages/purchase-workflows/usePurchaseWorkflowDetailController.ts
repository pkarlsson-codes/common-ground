import { useEffect, useState } from 'react'
import type { PurchaseWorkflowResponse } from '../../api'
import {
  useFailPurchase,
  useRecordAppraisalCompleted,
  useRecordDisclosureAcknowledged,
  useRecordEligibilityVerified,
  useRecordFinancingCommitted,
  useRecordInspectionCompleted,
  useRecordLeaseExecuted,
  useRecordPurchaseClosed,
  useRecordTitleCleared,
  useScheduleClosing,
  useWithdrawPurchase,
} from '../../hooks/purchaseWorkflows'
import type {
  FailWorkflowForm,
  RecordClosedForm,
  RecordAppraisalForm,
  RecordDisclosureForm,
  RecordEligibilityForm,
  RecordFinancingForm,
  RecordInspectionForm,
  RecordLeaseExecutedForm,
  RecordTitleForm,
  ScheduleClosingForm,
  WithdrawWorkflowForm,
} from '../../hooks/types'
import { usePurchaseWorkflowActions } from './usePurchaseWorkflowActions'
import { trustToday } from '../../lib/calendarDate'

export type TrackKey = 'eligibility' | 'appraisal' | 'financing' | 'inspection' | 'title' | 'leaseExecution' | 'disclosure' | 'closing'

const today = () => trustToday()

/** The eligibility form as typed. A null income or size is untouched and shows what the household declared. */
type EligibilityDraft = Omit<RecordEligibilityForm, 'verifiedAnnualIncome' | 'verifiedHouseholdSize'> & {
  verifiedAnnualIncome: string | null
  verifiedHouseholdSize: string | null
}

const emptyEligibility = (): EligibilityDraft => ({ verifiedDate: today(), verifiedAnnualIncome: null, verifiedHouseholdSize: null, notes: '', incomeFiles: [] })

/** Fills untouched fields with what the household declared when selected, or leaves them empty when it wasn't. */
const resolveEligibility = (draft: EligibilityDraft, detail: PurchaseWorkflowResponse | undefined): RecordEligibilityForm => ({
  ...draft,
  verifiedAnnualIncome: draft.verifiedAnnualIncome ?? (detail?.declaredAnnualIncome == null ? '' : String(detail.declaredAnnualIncome)),
  verifiedHouseholdSize: draft.verifiedHouseholdSize ?? (detail?.declaredHouseholdSize == null ? '' : String(detail.declaredHouseholdSize)),
})
const emptyAppraisal = (): RecordAppraisalForm => ({ appraisedValue: 0, appraisalDate: today(), appraiserName: '', notes: '' })
const emptyFinancing = (): RecordFinancingForm => ({ lenderName: '', committedAmount: 0, preapprovalLetterDate: today() })
const emptyInspection = (): RecordInspectionForm => ({ inspectionDate: today(), inspectorName: '', notes: '' })
const emptyTitle = (): RecordTitleForm => ({ clearedDate: today(), titleCompanyName: '' })
const emptyLeaseExec = (): RecordLeaseExecutedForm => ({ signedDate: today() })
const emptyDisclosure = (): RecordDisclosureForm => ({ acknowledgedDate: today(), notes: '', file: null })
const emptySchedule = (): ScheduleClosingForm => ({ scheduledDate: today() })
const emptyClose = (): RecordClosedForm => ({ actualClosingDate: today(), closingNotes: '' })
const emptyFail = (): FailWorkflowForm => ({ reason: '' })
const emptyWithdraw = (): WithdrawWorkflowForm => ({ reason: '' })

interface UsePurchaseWorkflowDetailControllerParams {
  selectedId: string | null
  detail: PurchaseWorkflowResponse | undefined
  showToast: (msg: string, kind?: 'success' | 'error') => void
  isDetailOpen: boolean
}

export function usePurchaseWorkflowDetailController({
  selectedId,
  detail,
  showToast,
  isDetailOpen,
}: UsePurchaseWorkflowDetailControllerParams) {
  const [eligForm, setEligForm] = useState<EligibilityDraft>(emptyEligibility)
  const [appraisalForm, setAppraisalForm] = useState<RecordAppraisalForm>(emptyAppraisal)
  const [finForm, setFinForm] = useState<RecordFinancingForm>(emptyFinancing)
  const [inspForm, setInspForm] = useState<RecordInspectionForm>(emptyInspection)
  const [titleForm, setTitleForm] = useState<RecordTitleForm>(emptyTitle)
  const [leaseExecForm, setLeaseExecForm] = useState<RecordLeaseExecutedForm>(emptyLeaseExec)
  const [disclosureForm, setDisclosureForm] = useState<RecordDisclosureForm>(emptyDisclosure)
  const [scheduleForm, setScheduleForm] = useState<ScheduleClosingForm>(emptySchedule)
  const [closeForm, setCloseForm] = useState<RecordClosedForm>(emptyClose)
  const [failForm, setFailForm] = useState<FailWorkflowForm>(emptyFail)
  const [withdrawForm, setWithdrawForm] = useState<WithdrawWorkflowForm>(emptyWithdraw)
  const [expandedTracks, setExpandedTracks] = useState<Set<TrackKey>>(() => new Set())

  const eligMutation = useRecordEligibilityVerified()
  const appraisalMutation = useRecordAppraisalCompleted()
  const finMutation = useRecordFinancingCommitted()
  const inspMutation = useRecordInspectionCompleted()
  const titleMutation = useRecordTitleCleared()
  const leaseExecMutation = useRecordLeaseExecuted()
  const disclosureMutation = useRecordDisclosureAcknowledged()
  const scheduleMutation = useScheduleClosing()
  const closeMutation = useRecordPurchaseClosed()
  const failMutation = useFailPurchase()
  const withdrawMutation = useWithdrawPurchase()

  useEffect(() => {
    if (!isDetailOpen) {
      return
    }

    setEligForm(emptyEligibility())
    setAppraisalForm(emptyAppraisal())
    setFinForm(emptyFinancing())
    setInspForm(emptyInspection())
    setTitleForm(emptyTitle())
    setLeaseExecForm(emptyLeaseExec())
    setDisclosureForm(emptyDisclosure())
    setScheduleForm(emptySchedule())
    setCloseForm(emptyClose())
    setFailForm(emptyFail())
    setWithdrawForm(emptyWithdraw())
    setExpandedTracks(new Set())
  }, [isDetailOpen, selectedId])

  const toggleTrack = (key: TrackKey) => {
    setExpandedTracks(prev => {
      const next = new Set(prev)
      if (next.has(key)) {
        next.delete(key)
      } else {
        next.add(key)
      }
      return next
    })
  }

  const collapseTrack = (key: TrackKey) => {
    setExpandedTracks(prev => {
      if (!prev.has(key)) return prev
      const next = new Set(prev)
      next.delete(key)
      return next
    })
  }

  const actions = usePurchaseWorkflowActions({
    selectedId,
    detail,
    showToast,
    eligMutation,
    appraisalMutation,
    finMutation,
    inspMutation,
    titleMutation,
    leaseExecMutation,
    disclosureMutation,
    scheduleMutation,
    closeMutation,
    failMutation,
    withdrawMutation,
    onTrackSuccess: collapseTrack,
  })

  return {
    canAct: actions.canAct,
    canSchedule: actions.canSchedule,
    canClose: actions.canClose,
    hasWorkflowAction: actions.hasWorkflowAction,
    expandedTracks,
    toggleTrack,
    collapseTrack,
    eligForm: resolveEligibility(eligForm, detail),
    setEligForm,
    appraisalForm,
    setAppraisalForm,
    finForm,
    setFinForm,
    inspForm,
    setInspForm,
    titleForm,
    setTitleForm,
    leaseExecForm,
    setLeaseExecForm,
    disclosureForm,
    setDisclosureForm,
    scheduleForm,
    setScheduleForm,
    closeForm,
    setCloseForm,
    failForm,
    setFailForm,
    withdrawForm,
    setWithdrawForm,
    handleElig: actions.handleElig,
    handleAppraisal: actions.handleAppraisal,
    handleFin: actions.handleFin,
    handleInsp: actions.handleInsp,
    handleTitle: actions.handleTitle,
    handleLeaseEx: actions.handleLeaseEx,
    handleDisclosure: actions.handleDisclosure,
    handleSchedule: actions.handleSchedule,
    handleClose: actions.handleClose,
    handleFail: actions.handleFail,
    handleWithdraw: actions.handleWithdraw,
    eligPending: eligMutation.isPending,
    appraisalPending: appraisalMutation.isPending,
    finPending: finMutation.isPending,
    inspPending: inspMutation.isPending,
    titlePending: titleMutation.isPending,
    leaseExecPending: leaseExecMutation.isPending,
    disclosurePending: disclosureMutation.isPending,
    schedulePending: scheduleMutation.isPending,
    scheduleError: scheduleMutation.isError,
    closePending: closeMutation.isPending,
    closeError: closeMutation.isError,
    failPending: failMutation.isPending,
    withdrawPending: withdrawMutation.isPending,
  }
}
