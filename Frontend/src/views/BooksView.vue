<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'

import type { BookDto, BookListItemDto } from '@/api/generated'
import BookEditorDialog from '@/components/BookEditorDialog.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
import { getApiErrorMessage } from '@/services/apiError'
import { booksService } from '@/services/books'
import { useUiStore } from '@/stores/ui'
import { formatAuthors } from '@/utils/format'

const router = useRouter()
const ui = useUiStore()
const books = ref<BookListItemDto[]>([])
const query = ref('')
const page = ref(1)
const pageCount = ref(0)
const totalCount = ref(0)
const loading = ref(false)
const editorOpen = ref(false)
const deleteDialogOpen = ref(false)
const deleting = ref(false)
const selectedBook = ref<BookListItemDto | null>(null)

async function loadBooks(): Promise<void> {
  loading.value = true
  try {
    const result = await booksService.search(query.value.trim(), page.value, 12)
    books.value = result.items ?? []
    pageCount.value = result.pageCount ?? 0
    totalCount.value = result.totalCount ?? 0
  } catch (error) {
    ui.error(getApiErrorMessage(error))
  } finally {
    loading.value = false
  }
}

function search(): void {
  if (page.value !== 1) {
    page.value = 1
  } else {
    void loadBooks()
  }
}

function openCreate(): void {
  selectedBook.value = null
  editorOpen.value = true
}

async function openEdit(book: BookListItemDto): Promise<void> {
  if (!book.id) return
  loading.value = true
  try {
    selectedBook.value = await booksService.getById(book.id)
    editorOpen.value = true
  } catch (error) {
    ui.error(getApiErrorMessage(error))
  } finally {
    loading.value = false
  }
}

function openDelete(book: BookListItemDto): void {
  selectedBook.value = book
  deleteDialogOpen.value = true
}

async function removeBook(): Promise<void> {
  if (!selectedBook.value?.id) return
  deleting.value = true
  try {
    await booksService.remove(selectedBook.value.id, selectedBook.value.rowVersion)
    ui.success('Книга удалена из библиотеки.')
    deleteDialogOpen.value = false
    selectedBook.value = null
    if (books.value.length === 1 && page.value > 1) page.value -= 1
    else await loadBooks()
  } catch (error) {
    ui.error(getApiErrorMessage(error))
  } finally {
    deleting.value = false
  }
}

async function afterSave(_book: BookDto): Promise<void> {
  await loadBooks()
}

function viewBook(book: BookListItemDto): void {
  if (book.id) void router.push({ name: 'book-details', params: { id: book.id } })
}

watch(page, () => void loadBooks())
onMounted(() => void loadBooks())
</script>

<template>
  <v-container class="page-container">
    <section class="hero-section">
      <div>
        <span class="eyebrow">Личная коллекция</span>
        <h1>Книжная полка</h1>
        <p>Поиск по названию, автору или тексту оглавления</p>
      </div>
      <v-btn color="primary" prepend-icon="mdi-plus" size="large" variant="flat" @click="openCreate">
        Добавить книгу
      </v-btn>
    </section>

    <v-card class="search-panel" elevation="0">
      <v-form class="search-form" @submit.prevent="search">
        <v-text-field
          v-model="query"
          clearable
          hide-details
          label="Поиск в библиотеке"
          placeholder="Например, Булгаков или Глава первая"
          prepend-inner-icon="mdi-magnify"
          @click:clear="search"
        />
        <v-btn color="primary" height="48" type="submit" variant="tonal">Найти</v-btn>
      </v-form>
      <span class="result-count">{{ totalCount }} {{ totalCount === 1 ? 'книга' : 'книг' }}</span>
    </v-card>

    <v-progress-linear v-if="loading" color="primary" indeterminate rounded />

    <v-card v-if="!loading && books.length === 0" class="empty-state" elevation="0">
      <v-icon color="primary" icon="mdi-bookshelf" size="64" />
      <h2>{{ query ? 'Ничего не найдено' : 'Полка пока пуста' }}</h2>
      <p>{{ query ? 'Попробуйте изменить поисковый запрос.' : 'Добавьте первую книгу в свою коллекцию.' }}</p>
      <v-btn v-if="!query" color="primary" variant="tonal" @click="openCreate">Добавить книгу</v-btn>
    </v-card>

    <div v-else class="book-grid" aria-live="polite">
      <v-card v-for="book in books" :key="book.id" class="book-card" elevation="0" @click="viewBook(book)">
        <div class="book-spine" aria-hidden="true" />
        <div class="book-card__content">
          <div class="book-card__topline">
            <v-chip color="secondary" size="small" variant="tonal">{{ book.publicationYear }} год</v-chip>
            <v-menu>
              <template #activator="{ props: menuProps }">
                <v-btn
                  aria-label="Действия с книгой"
                  icon="mdi-dots-horizontal"
                  size="small"
                  variant="text"
                  v-bind="menuProps"
                  @click.stop
                />
              </template>
              <v-list density="compact">
                <v-list-item prepend-icon="mdi-pencil-outline" title="Редактировать" @click="openEdit(book)" />
                <v-list-item
                  base-color="error"
                  prepend-icon="mdi-delete-outline"
                  title="Удалить"
                  @click="openDelete(book)"
                />
              </v-list>
            </v-menu>
          </div>
          <div class="book-icon"><v-icon icon="mdi-book-open-page-variant-outline" /></div>
          <h2>{{ book.title }}</h2>
          <p>{{ formatAuthors(book.authors) }}</p>
          <div class="book-card__footer">
            <span>Открыть карточку</span>
            <v-icon icon="mdi-arrow-right" size="small" />
          </div>
        </div>
      </v-card>
    </div>

    <v-pagination
      v-if="pageCount > 1"
      v-model="page"
      class="mt-8"
      color="primary"
      :length="pageCount"
      :total-visible="7"
    />

    <BookEditorDialog
      v-model="editorOpen"
      :book="selectedBook as BookDto | null"
      @saved="afterSave"
    />
    <ConfirmDialog
      v-model="deleteDialogOpen"
      :loading="deleting"
      :text="`Книга «${selectedBook?.title ?? ''}» будет удалена из библиотеки.`"
      title="Удалить книгу?"
      @confirm="removeBook"
    />
  </v-container>
</template>

<style scoped>
.search-panel {
  display: flex;
  align-items: center;
  gap: 22px;
  padding: 18px;
  margin-bottom: 26px;
  border: 1px solid rgba(36, 91, 82, 0.12);
}

.search-form {
  display: grid;
  grid-template-columns: minmax(240px, 680px) auto;
  gap: 10px;
  flex: 1;
}

.result-count {
  color: #748079;
  white-space: nowrap;
}

.book-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 20px;
  margin-top: 10px;
}

.book-card {
  position: relative;
  min-height: 285px;
  overflow: hidden;
  border: 1px solid rgba(36, 91, 82, 0.1);
  cursor: pointer;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.book-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 16px 34px rgba(27, 58, 51, 0.1) !important;
}

.book-spine {
  position: absolute;
  inset: 0 auto 0 0;
  width: 8px;
  background: linear-gradient(#245b52, #173c36);
}

.book-card__content {
  display: flex;
  flex-direction: column;
  height: 100%;
  padding: 20px 22px 18px 28px;
}

.book-card__topline,
.book-card__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.book-icon {
  display: grid;
  place-items: center;
  width: 50px;
  height: 50px;
  margin: 22px 0 18px;
  border-radius: 16px;
  color: #245b52;
  background: #e5eee9;
  font-size: 25px;
}

.book-card h2 {
  color: #263b35;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: clamp(20px, 2vw, 25px);
  line-height: 1.2;
}

.book-card p {
  margin: 8px 0 22px;
  color: #6d7872;
}

.book-card__footer {
  padding-top: 15px;
  margin-top: auto;
  border-top: 1px solid #e4e5e0;
  color: #245b52;
  font-size: 13px;
  font-weight: 600;
}

@media (max-width: 900px) {
  .book-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 600px) {
  .search-panel,
  .hero-section {
    align-items: stretch;
    flex-direction: column;
  }

  .search-form {
    grid-template-columns: 1fr;
  }

  .book-grid {
    grid-template-columns: 1fr;
  }
}
</style>
