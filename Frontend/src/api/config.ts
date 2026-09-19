import { OpenAPI } from '@/api/generated'

export function configureApi(): void {
  OpenAPI.BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') ?? ''
  OpenAPI.WITH_CREDENTIALS = false
}
