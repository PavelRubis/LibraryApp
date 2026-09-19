import {
  AuthorsService as GeneratedAuthorsService,
  type AuthorDto,
  type AuthorDtoPagedResult,
  type CreateAuthorRequest,
  type UpdateAuthorRequest,
} from '@/api/generated'

function toEtag(rowVersion: string | null | undefined): string {
  if (!rowVersion) {
    throw new Error('Версия записи отсутствует. Обновите список и повторите действие.')
  }

  return `"${rowVersion}"`
}

export const authorsService = {
  search(q = '', page = 1, pageSize = 20): Promise<AuthorDtoPagedResult> {
    return GeneratedAuthorsService.getApiAuthors({ q: q || undefined, page, pageSize })
  },

  getById(id: string): Promise<AuthorDto> {
    return GeneratedAuthorsService.getApiAuthors1({ id })
  },

  create(request: CreateAuthorRequest): Promise<AuthorDto> {
    return GeneratedAuthorsService.postApiAuthors({ requestBody: request })
  },

  update(id: string, request: UpdateAuthorRequest): Promise<AuthorDto> {
    return GeneratedAuthorsService.putApiAuthors({ id, requestBody: request })
  },

  remove(id: string, rowVersion: string | null | undefined): Promise<void> {
    return GeneratedAuthorsService.deleteApiAuthors({ id, ifMatch: toEtag(rowVersion) })
  },
}
