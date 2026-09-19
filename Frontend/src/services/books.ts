import {
  BooksService as GeneratedBooksService,
  type BookDto,
  type BookListItemDtoPagedResult,
  type CreateBookRequest,
  type UpdateBookRequest,
} from '@/api/generated'

function toEtag(rowVersion: string | null | undefined): string {
  if (!rowVersion) {
    throw new Error('Версия записи отсутствует. Обновите список и повторите действие.')
  }

  return `"${rowVersion}"`
}

export const booksService = {
  search(q = '', page = 1, pageSize = 12): Promise<BookListItemDtoPagedResult> {
    return GeneratedBooksService.getApiBooks({ q: q || undefined, page, pageSize })
  },

  getById(id: string): Promise<BookDto> {
    return GeneratedBooksService.getApiBooks1({ id })
  },

  create(request: CreateBookRequest): Promise<BookDto> {
    return GeneratedBooksService.postApiBooks({ requestBody: request })
  },

  update(id: string, request: UpdateBookRequest): Promise<BookDto> {
    return GeneratedBooksService.putApiBooks({ id, requestBody: request })
  },

  remove(id: string, rowVersion: string | null | undefined): Promise<void> {
    return GeneratedBooksService.deleteApiBooks({ id, ifMatch: toEtag(rowVersion) })
  },
}
