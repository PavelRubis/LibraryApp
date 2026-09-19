<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { VForm } from 'vuetify/components'

import type { AuthorDto } from '@/api/generated'
import { getApiErrorMessage } from '@/services/apiError'
import { authorsService } from '@/services/authors'
import { useUiStore } from '@/stores/ui'

const props = defineProps<{
  modelValue: boolean
  author?: AuthorDto | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  saved: [author: AuthorDto]
}>()

const ui = useUiStore()
const form = ref<InstanceType<typeof VForm> | null>(null)
const name = ref('')
const saving = ref(false)
const isEditing = computed(() => Boolean(props.author?.id))

async function save(): Promise<void> {
  const validation = await form.value?.validate()
  if (!validation?.valid) return

  saving.value = true
  try {
    let result: AuthorDto
    if (isEditing.value && props.author?.id) {
      if (!props.author.rowVersion) throw new Error('Не удалось определить версию автора.')
      result = await authorsService.update(props.author.id, {
        name: name.value.trim(),
        rowVersion: props.author.rowVersion,
      })
    } else {
      result = await authorsService.create({ name: name.value.trim() })
    }

    ui.success(isEditing.value ? 'Автор обновлён.' : 'Автор добавлен.')
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
    if (isOpen) name.value = props.author?.name ?? ''
  },
)
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="520"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card>
      <v-card-title class="px-6 pt-6">{{ isEditing ? 'Редактирование автора' : 'Новый автор' }}</v-card-title>
      <v-card-text class="px-6 pt-5">
        <v-form ref="form" @submit.prevent="save">
          <v-text-field
            v-model="name"
            autofocus
            counter="200"
            label="Имя автора"
            :rules="[
              (value: string) => Boolean(value?.trim()) || 'Укажите имя автора.',
              (value: string) => value.length <= 200 || 'Не более 200 символов.',
            ]"
          />
        </v-form>
      </v-card-text>
      <v-card-actions class="px-6 pb-6">
        <v-spacer />
        <v-btn :disabled="saving" variant="text" @click="emit('update:modelValue', false)">Отмена</v-btn>
        <v-btn color="primary" :loading="saving" variant="flat" @click="save">Сохранить</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
