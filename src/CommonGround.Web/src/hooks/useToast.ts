import { toast, type ExternalToast } from 'sonner'

type ToastOptions = Pick<ExternalToast, 'description' | 'duration' | 'action' | 'cancel'>

export function useToast() {
  return {
    success: (message: string, options?: ToastOptions) => toast.success(message, options),
    error: (message: string, options?: ToastOptions) => toast.error(message, options),
    info: (message: string, options?: ToastOptions) => toast.info(message, options),
    warning: (message: string, options?: ToastOptions) => toast.warning(message, options),
  }
}
