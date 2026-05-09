import { useState } from 'react'
import { useCalculateResaleEstimate } from '../../hooks/groundLeases'
import { useInitiatePurchaseWorkflow } from '../../hooks/purchaseWorkflows'
import type { InitiateWorkflowForm, ResaleEstimateForm } from '../../hooks/types'
import { getErrorMessage } from '../../lib/getErrorMessage'

const emptyEstimate = (): ResaleEstimateForm => ({
  originalPurchasePrice: 0,
  currentAppraisalValue: 0,
  originalAppraisalValue: 0,
  ownershipYears: 0,
  approvedImprovements: 0,
  currentIndexValue: 0,
})

const emptyInitiate = (groundLeaseId: string): InitiateWorkflowForm => ({
  groundLeaseId,
  buyerFullName: '',
  buyerEmail: '',
  buyerPhone: '',
  purchasePrice: 0,
})

interface Props {
  groundLeaseId: string
  onSuccess: (workflowId?: string) => void
}

const hasPurchaseWorkflowId = (value: unknown): value is { purchaseWorkflowId?: string } =>
  typeof value === 'object' && value !== null && 'purchaseWorkflowId' in value

export function useResaleFlowController({ groundLeaseId, onSuccess }: Props) {
  const [step, setStep] = useState<0 | 1 | 2>(0)

  const [estimateForm, setEstimateForm] = useState<ResaleEstimateForm>(emptyEstimate)
  const [estimateResult, setEstimateResult] = useState<number | null>(null)
  const [estimateError, setEstimateError] = useState<string | null>(null)

  const [initiateForm, setInitiateForm] = useState<InitiateWorkflowForm>(() => emptyInitiate(groundLeaseId))
  const [initiateError, setInitiateError] = useState<string | null>(null)

  const estimateMutation = useCalculateResaleEstimate()
  const initiateMutation = useInitiatePurchaseWorkflow()

  const nextStep = () => setStep(s => Math.min(s + 1, 2) as 0 | 1 | 2)
  const prevStep = () => setStep(s => Math.max(s - 1, 0) as 0 | 1 | 2)

  const handleEstimate = async (e: React.FormEvent) => {
    e.preventDefault()
    setEstimateError(null)
    try {
      const result = await estimateMutation.mutateAsync({ groundLeaseId, form: estimateForm })
      const price = result.maxResalePrice ?? 0
      setEstimateResult(price)
      setInitiateForm(f => ({ ...f, purchasePrice: price }))
      nextStep()
    } catch (error) {
      setEstimateError(await getErrorMessage(error, 'Calculation failed. Check your inputs.'))
    }
  }

  const handleInitiate = async (e: React.FormEvent) => {
    e.preventDefault()
    setInitiateError(null)
    try {
      const result = await initiateMutation.mutateAsync({
        ...initiateForm,
        purchasePrice: Number(initiateForm.purchasePrice),
      })
      onSuccess(hasPurchaseWorkflowId(result) ? result.purchaseWorkflowId : undefined)
    } catch (error) {
      setInitiateError(await getErrorMessage(error, 'Failed to initiate workflow.'))
    }
  }

  return {
    step,
    nextStep,
    prevStep,
    estimateForm,
    setEstimateForm,
    estimateResult,
    estimateError,
    estimatePending: estimateMutation.isPending,
    initiateForm,
    setInitiateForm,
    initiateError,
    initiatePending: initiateMutation.isPending,
    handleEstimate,
    handleInitiate,
  }
}
