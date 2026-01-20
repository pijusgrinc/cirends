<template>
  <footer class="app-footer">
    <div class="footer-content">
      <div class="footer-section">
        <h4>Apie Cirends</h4>
        <p>Veiklų organizavimo ir išlaidų valdymo sistema draugų grupėms ir ne tik.</p>
      </div>

      <div class="footer-section">
        <h4>Nuorodos</h4>
        <ul class="footer-links">
          <li><router-link to="/privacy">Privatumas</router-link></li>
          <li><router-link to="/terms">Naudojimo sąlygos</router-link></li>
          <li><a href="mailto:info@cirends.lt">Kontaktai</a></li>
          <li><a href="https://github.com/pijusgrinc/cirends">GitHub</a></li>
        </ul>
      </div>
      <div v-if="isAuthenticated" class="footer-section">
        <h4>Greitos nuorodos</h4>
        <ul class="footer-links">
          <li><router-link to="/dashboard">Pagrindinis</router-link></li>
          <li><router-link to="/activities">Veiklos</router-link></li>
          <li><router-link to="/invitations">Kvietimai</router-link></li>
          <li><router-link to="/profile">Profilis</router-link></li>
        </ul>
      </div>
    </div>

    <div class="footer-bottom">
      <p>&copy; {{ new Date().getFullYear() }} Cirends. Visos teisės saugomos.</p>
    </div>
  </footer>
</template>

<script setup>
import { computed } from 'vue'
import { useAuthStore } from '@/stores'

const authStore = useAuthStore()
const isAuthenticated = computed(() => authStore.isAuthenticated)
</script>

<style scoped>
.app-footer {
  background: linear-gradient(180deg, #ffffff 0%, #f8f9fb 100%);
  color: var(--text-secondary);
  padding: 3rem 1rem 1.5rem;
  margin-top: 4rem;
  border-top: 1px solid var(--border);
}

.footer-content {
  max-width: 1200px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 3rem;
  margin-bottom: 2rem;
}

.footer-section h4 {
  color: var(--primary);
  margin-bottom: 1rem;
  font-family: 'Poppins', sans-serif;
  font-weight: 600;
}

.footer-section p {
  color: var(--text-secondary);
  font-size: 0.9rem;
  line-height: 1.6;
  margin-bottom: 1rem;
}

.footer-links {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.footer-links a {
  color: var(--text-secondary);
  text-decoration: none;
  transition: var(--transition);
  font-size: 0.9rem;
}

.footer-links a:hover {
  color: var(--primary);
}

.social-links {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
}

.social-link {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  background: rgba(102, 126, 234, 0.1);
  border-radius: 50%;
  color: var(--primary);
  font-weight: bold;
  transition: var(--transition);
  text-decoration: none;
}

.social-link:hover {
  background: var(--primary);
  color: white;
  transform: translateY(-3px);
}

.footer-bottom {
  max-width: 1200px;
  margin: 0 auto;
  padding-top: 2rem;
  border-top: 1px solid var(--border);
  text-align: center;
  color: var(--text-secondary);
  font-size: 0.85rem;
}

.footer-bottom p {
  margin: 0.5rem 0;
}

.heart-icon {
  width: 14px;
  height: 14px;
  display: inline;
  color: var(--danger);
  margin: 0 0.3rem;
}

@media (max-width: 768px) {
  .footer-content {
    grid-template-columns: 1fr;
    gap: 2rem;
  }

  .social-links {
    justify-content: center;
  }
}
</style>
