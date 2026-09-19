import type { AuthorDto } from '@/api/generated'

export function formatAuthors(authors: AuthorDto[] | null | undefined): string {
  const names = authors?.map((author) => author.name).filter(Boolean) ?? []
  return names.length > 0 ? names.join(', ') : 'Автор не указан'
}

export function formatDate(value: string | null | undefined): string {
  if (!value) return '—'

  return new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: 'long',
    year: 'numeric',
  }).format(new Date(value))
}
