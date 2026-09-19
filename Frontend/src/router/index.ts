import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/books',
    },
    {
      path: '/books',
      name: 'books',
      component: () => import('@/views/BooksView.vue'),
    },
    {
      path: '/books/:id',
      name: 'book-details',
      component: () => import('@/views/BookDetailsView.vue'),
      props: true,
    },
    {
      path: '/authors',
      name: 'authors',
      component: () => import('@/views/AuthorsView.vue'),
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/books',
    },
  ],
  scrollBehavior: () => ({ top: 0 }),
})

export default router
