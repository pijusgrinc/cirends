import { createRouter, createWebHistory } from 'vue-router'
import { getToken } from '../api.js'
import { useAuthStore } from '@/stores'

// Views
import LandingView from '../views/LandingView.vue'
import LoginView from '../views/LoginView.vue'
import RegisterView from '../views/RegisterView.vue'
import DashboardView from '../views/DashboardView.vue'
import ActivitiesView from '../views/ActivitiesView.vue'
import ActivityDetailView from '../views/ActivityDetailView.vue'
import InvitationsView from '../views/InvitationsView.vue'
import MyExpensesView from '../views/MyExpensesView.vue'
import ProfileView from '../views/ProfileView.vue'
import AdminView from '../views/AdminView.vue'
import AdminUsersView from '../views/AdminUsersView.vue'
import NotFoundView from '../views/NotFoundView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: LandingView,
      meta: { requiresGuest: true },
      beforeEnter: (to, from, next) => {
        if (getToken()) {
          next('/dashboard')
        } else {
          next()
        }
      }
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { layout: 'auth', requiresGuest: true }
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView,
      meta: { layout: 'auth', requiresGuest: true }
    },
    {
      path: '/dashboard',
      name: 'dashboard',
      component: DashboardView,
      meta: { requiresAuth: true }
    },
    {
      path: '/activities',
      name: 'activities',
      component: ActivitiesView,
      meta: { requiresAuth: true }
    },
    {
      path: '/activities/:id',
      name: 'activity-detail',
      component: ActivityDetailView,
      meta: { requiresAuth: true }
    },
    {
      path: '/invitations',
      name: 'invitations',
      component: InvitationsView,
      meta: { requiresAuth: true }
    },
    {
      path: '/my-expenses',
      name: 'my-expenses',
      component: MyExpensesView,
      meta: { requiresAuth: true }
    },
    {
      path: '/profile',
      name: 'profile',
      component: ProfileView,
      meta: { requiresAuth: true }
    },
    {
      path: '/admin',
      name: 'admin',
      component: AdminView,
      meta: { requiresAuth: true, requiresAdmin: true }
    },
    {
      path: '/admin/users',
      name: 'admin-users',
      component: AdminUsersView,
      meta: { requiresAuth: true, requiresAdmin: true }
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: NotFoundView
    }
  ]
})

// Navigation guard
router.beforeEach((to, from, next) => {
  const isAuthenticated = !!getToken()

  if (to.meta.requiresAuth && !isAuthenticated) {
    next('/login')
  } else if (to.meta.requiresGuest && isAuthenticated) {
    next('/dashboard')
  } else if (to.meta.requiresAdmin) {
    // Check if user is admin
    const authStore = useAuthStore()
    if (!authStore.isAdmin) {
      next('/dashboard')
    } else {
      next()
    }
  } else {
    next()
  }
})

export default router
