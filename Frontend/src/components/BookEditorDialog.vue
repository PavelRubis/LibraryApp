<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { VForm } from 'vuetify/components'

import type { AuthorDto, BookDto } from '@/api/generated'
import RichTextEditor from '@/components/RichTextEditor.vue'
import { getApiErrorMessage } from '@/services/apiError'
import { authorsService } from '@/services/authors'
import { booksService } from '@/services/books'
import { useUiStore } from '@/stores/ui'
import { validateTocXml } from '@/utils/xml'

const props = defineProps<{
  modelValue: boolean
  book?: BookDto | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  saved: [book: BookDto]
}>()

interface BookFormModel {
  title: string
  publicationYear: number
  authorIds: string[]
  tableOfContentsXml: string
}

const currentYear = new Date().getFullYear()
const ui = useUiStore()
const form = ref<InstanceType<typeof VForm> | null>(null)
const saving = ref(false)
const loadingAuthors = ref(false)
const authors = ref<AuthorDto[]>([])
const model = ref<BookFormModel>(emptyModel())

const isEditing = computed(() => Boolean(props.book?.id))
const dialogTitle = computed(() => (isEditing.value ? 'Редактирование книги' : 'Новая книга'))

function emptyModel(): BookFormModel {
  return {
    title: '',
    publicationYear: currentYear,
    authorIds: [],
    tableOfContentsXml: '<toc><p></p></toc>',
  }
}

function resetModel(): void {
  if (!props.book) {
    model.value = emptyModel()
    return
  }

  model.value = {
    title: props.book.title ?? '',
    publicationYear: props.book.publicationYear ?? currentYear,
    authorIds: props.book.authors?.flatMap((author) => (author.id ? [author.id] : [])) ?? [],
    tableOfContentsXml: props.book.tableOfContentsXml ?? '<toc><p></p></toc>',
  }
}

async function loadAuthors(): Promise<void> {
  loadingAuthors.value = true
  try {
    const response = await authorsService.search('', 1, 100)
    authors.value = response.items ?? []
  } catch (error) {
    ui.error(getApiErrorMessage(error))
  } finally {
    loadingAuthors.value = false
  }
}

async function save(): Promise<void> {
  const validation = await form.value?.validate()
  const xmlValidation = validateTocXml(model.value.tableOfContentsXml)
  if (!validation?.valid || xmlValidation !== true) {
    ui.error(typeof xmlValidation === 'string' ? xmlValidation : 'Проверьте заполнение полей.')
    return
  }

  saving.value = true
  try {
    let result: BookDto
    if (isEditing.value && props.book?.id) {
      if (!props.book.rowVersion) throw new Error('Не удалось определить версию книги.')
      result = await booksService.update(props.book.id, {
        title: model.value.title.trim(),
        publicationYear: Number(model.value.publicationYear),
        authorIds: model.value.authorIds,
        tableOfContentsXml: model.value.tableOfContentsXml,
        rowVersion: props.book.rowVersion,
      })
    } else {
      result = await booksService.create({
        title: model.value.title.trim(),
        publicationYear: Number(model.value.publicationYear),
        authorIds: model.value.authorIds,
        tableOfContentsXml: model.value.tableOfContentsXml,
      })
    }

    ui.success(isEditing.value ? 'Книга обновлена.' : 'Книга добавлена в библиотеку.')
    emit('saved', result)
    emit('update:modelValue', false)
  } catch (error) {
    ui.error(getApiErrorMessage(error))
  } finally {
    saving.value = false
  }
}

watch(
  () => props.modelValue,
  (isOpen) => {
    if (!isOpen) return
    resetModel()
    void loadAuthors()
  },
)
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="820"
    scrollable
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card>
      <v-card-title class="dialog-title px-6 pt-6 pb-2">
        <span>
          <small>{{ isEditing ? 'Карточка книги' : 'Пополнение коллекции' }}</small>
          {{ dialogTitle }}
        </span>
        <v-btn
          aria-label="Закрыть"
          icon="mdi-close"
          size="small"
          variant="text"
          @click="emit('update:modelValue', false)"
        />
      </v-card-title>

      <v-card-text class="px-6 py-4">
        <v-form ref="form" @submit.prevent="save">
          <v-row>
            <v-col cols="12" md="8">
              <v-text-field
                v-model="model.title"
                autofocus
                counter="300"
                label="Название"
                :rules="[
                  (value: string) => Boolean(value?.trim()) || 'Укажите название.',
                  (value: string) => value.length <= 300 || 'Не более 300 символов.',
                ]"
              />
            </v-col>
            <v-col cols="12" md="4">
              <v-text-field
                v-model.number="model.publicationYear"
                label="Год издания"
                type="number"
                :rules="[
                  (value: number) => (value >= 1 && value <= currentYear) || `Год от 1 до ${currentYear}.`,
                ]"
              />
            </v-col>
            <v-col cols="12">
              <v-autocomplete
                v-model="model.authorIds"
                chips
                closable-chips
                item-title="name"
                item-value="id"
                :items="authors"
                label="Авторы"
                :loading="loadingAuthors"
                multiple
                no-data-text="Авторы не найдены"
                :rules="[
                  (value: string[]) => value.length > 0 || 'Выберите хотя бы одного автора.',
                  (value: string[]) => value.length <= 10 || 'Не более 10 авторов.',
                ]"
              >
                <template #append-item>
                  <v-list-item to="/authors" title="Управление авторами" prepend-icon="mdi-account-plus" />
                </template>
              </v-autocomplete>
            </v-col>
            <v-col cols="12">
              <RichTextEditor v-model="model.tableOfContentsXml" label="Оглавление" />
            </v-col>
          </v-row>
        </v-form>
      </v-card-text>

      <v-card-actions class="px-6 pb-6 pt-2">
        <v-spacer />
        <v-btn :disabled="saving" variant="text" @click="emit('update:modelValue', false)">Отмена</v-btn>
        <v-btn color="primary" prepend-icon="mdi-content-save" :loading="saving" variant="flat" @click="save">
          Сохранить
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.dialog-title {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: 25px;
}

.dialog-title span {
  display: flex;
  flex-direction: column;
}

.dialog-title small {
  margin-bottom: 5px;
  color: #78837d;
  font-family: Inter, system-ui, sans-serif;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}
</style>
