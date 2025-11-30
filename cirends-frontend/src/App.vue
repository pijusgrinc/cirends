<script setup>
import { computed } from 'vue'
import AppHeader from './components/AppHeader.vue'
import AppNav from './components/AppNav.vue'
import AppFooter from './components/AppFooter.vue'
import Toast from './components/common/Toast.vue'
import { useAuthStore } from '@/stores'

const authStore = useAuthStore()
const isLoggedIn = computed(() => authStore.isAuthenticated)
</script>

<template>
  <div >
    <AppHeader />
    <AppNav v-if="isLoggedIn" />
    <main>
      <router-view />
    </main>
    <AppFooter />
    <Toast />
  </div>
</template>

<style scoped>
.app {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}


@media (max-width: 768px) {
  .app-main {
    padding: 1rem 0;
  }
}
</style>