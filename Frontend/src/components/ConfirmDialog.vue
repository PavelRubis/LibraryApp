<script setup lang="ts">
withDefaults(
  defineProps<{
    modelValue: boolean
    title?: string
    text?: string
    loading?: boolean
  }>(),
  {
    title: 'Подтвердите действие',
    text: 'Это действие нельзя отменить.',
    loading: false,
  },
)

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirm: []
}>()
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="480"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <v-card>
      <v-card-title class="d-flex align-center ga-3 pa-6 pb-2">
        <v-icon color="error" icon="mdi-alert-circle-outline" />
        {{ title }}
      </v-card-title>
      <v-card-text class="text-body-1 px-6">{{ text }}</v-card-text>
      <v-card-actions class="pa-6 pt-3">
        <v-spacer />
        <v-btn :disabled="loading" variant="text" @click="emit('update:modelValue', false)">
          Отмена
        </v-btn>
        <v-btn color="error" :loading="loading" variant="flat" @click="emit('confirm')">
          Удалить
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
