import { Permissions } from '@/constants/api/Permissions'
import { AuthService } from '@/services/auth-service'
import { createRouter, createWebHistory, type LocationQueryValue } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      redirect: { name: 'viewAdvertisements' }
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/user/LoginView.vue'),
      props: (route) => ({ redirect: !!route.query.redirect })
    },
    {
      path: '/logout',
      name: 'logout',
      beforeEnter: (_, from, next) => {
        AuthService.get().logout()

        if (from.meta.requiresPermission != null) {
          next({ name: 'home' })
        } else {
          next(from.fullPath)
        }
      },
      //Component is not displayed, guard always redirects to another page. Component is used to make this a valid route.
      component: () => import('../views/NotFound.vue')
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('../views/user/RegisterView.vue')
    },

    //Users
    {
      path: '/users/',
      children: [
        {
          path: 'view-profile/:id/',
          name: 'viewUserProfile',
          component: () => import('../views/user/ViewUserProfile.vue'),
          props: (route) => ({ id: toNumberOrUndefined(route.params.id) })
        },
        {
          path: 'manage',
          name: 'manageUsers',
          component: () => import('../views/user/ManageUsers.vue'),
          meta: {
            requiresPermission: Permissions.ViewAllUsers
          }
        },
        {
          path: 'create',
          name: 'createUser',
          component: () => import('../views/user/CreateUserForm.vue'),
          meta: {
            requiresPermission: Permissions.CreateUser
          }
        },
        {
          path: 'view/:userId',
          name: 'viewUser',
          component: () => import('../views/user/ViewUser.vue'),
          props: (route) => ({ userId: toNumberOrUndefined(route.params.userId) }),
          meta: {
            requiresPermission: Permissions.ViewAllUsers
          }
        },
        {
          path: 'edit/:userId',
          name: 'editUser',
          component: () => import('../views/user/EditUserForm.vue'),
          props: (route) => ({ userId: toNumberOrUndefined(route.params.userId) }),
          meta: {
            requiresPermission: Permissions.EditAnyUser
          }
        }
      ]
    },

    //Advertisements
    {
      path: '/advertisements',
      redirect: { name: 'viewAdvertisements' },
      children: [
        {
          path: 'search',
          name: 'searchAdvertisements',
          component: () => import('../views/advertisements/SearchAdvertisement.vue'),
          props: (route) => ({ search: route.query?.search })
        },
        {
          path: 'view',
          name: 'viewAdvertisements',
          component: () => import('../views/advertisements/ViewAdvertisements.vue')
        },
        {
          path: 'recently-viewed',
          name: 'recentlyViewedAdvertisements',
          component: () => import('../views/advertisements/RecentlyViewedAdvertisements.vue')
        },
        {
          path: 'bookmarked',
          name: 'bookmarkedAdvertisements',
          component: () => import('../views/advertisements/BookmarkedAdvertisements.vue'),
          meta: {
            requiresPermission: Permissions.ViewAdvertisementBookmarks
          }
        },
        {
          path: '/manage',
          name: 'manageOwnAdvertisements',
          component: () => import('../views/advertisements/ManageAdvertisements.vue'),
          props: () => ({ manageAll: false }),
          meta: {
            requiresPermission: Permissions.ViewOwnedAdvertisements
          }
        },
        {
          path: '/manage-all',
          name: 'manageAdvertisements',
          component: () => import('../views/advertisements/ManageAdvertisements.vue'),
          props: () => ({ manageAll: true }),
          meta: {
            requiresPermission: Permissions.ViewAllAdvertisements
          }
        }
      ]
    },

    //Advertisement
    {
      path: '/advertisement',
      redirect: { name: 'viewAdvertisements' },
      children: [
        {
          path: 'create-own',
          name: 'createOwnAdvertisement',
          component: () => import('../views/advertisement/CreateOrEditAdvertisement.vue'),
          meta: {
            requiresPermission: Permissions.CreateOwnedAdvertisement
          }
        },
        {
          path: 'create',
          name: 'createAdvertisement',
          component: () => import('../views/advertisement/CreateOrEditAdvertisement.vue'),
          props: () => ({ forAnyUser: true }),
          meta: {
            requiresPermission: Permissions.CreateAdvertisement
          }
        },
        {
          path: ':id/',
          redirect: (route) => ({ name: 'viewAdvertisement', params: route.params }),
          children: [
            {
              path: 'view',
              name: 'viewAdvertisement',
              component: () => import('../views/advertisement/ViewAdvertisement.vue'),
              props: (route) => ({ id: toNumberOrUndefined(route.params.id) })
            },
            {
              path: 'report',
              name: 'reportAdvertisement',
              component: () => import('../views/advertisement/ReportAdvertisement.vue'),
              props: (route) => ({ id: toNumberOrUndefined(route.params.id) })
            },
            {
              path: 'edit',
              name: 'editAdvertisement',
              component: () => import('../views/advertisement/CreateOrEditAdvertisement.vue'),
              props: (route) => ({ id: toNumberOrUndefined(route.params.id) }),
              meta: {
                requiresPermission: Permissions.EditOwnedAdvertisement
              }
            },
            {
              path: 'edit-any',
              name: 'editAnyAdvertisement',
              component: () => import('../views/advertisement/CreateOrEditAdvertisement.vue'),
              props: (route) => ({ id: toNumberOrUndefined(route.params.id), forAnyUser: true }),
              meta: {
                requiresPermission: Permissions.EditAnyAdvertisement
              }
            }
          ]
        }
      ]
    },

    {
      path: '/extend/:type/:ids',
      name: 'extend',
      component: () => import('../views/ExtendForm.vue'),
      props: (route) => ({
        ids: JSON.parse(firstParam(route.params.ids)),
        type: firstParam(route.params.type)
      })
    },

    {
      path: '/profile-info',
      redirect: { name: 'profileInfo' },
      children: [
        {
          path: 'view',
          name: 'profileInfo',
          component: () => import('../views/user/ProfileInfo.vue'),
          meta: {
            requiresPermission: Permissions.ViewOwnProfileInfo
          }
        },
        {
          path: 'edit',
          name: 'editProfileInfo',
          component: () => import('../views/user/EditProfileInfo.vue'),
          meta: {
            requiresPermission: Permissions.EditOwnProfileInfo
          }
        }
      ]
    },

    {
      path: '/change-password',
      name: 'changePassword',
      component: () => import('../views/user/ChangePassword.vue'),
      meta: {
        requiresPermission: Permissions.ChangeOwnPassword
      }
    },

    //Messages
    {
      path: '/view-messages/:chatId?',
      name: 'viewMessages',
      component: () => import('../views/messages/ViewMessages.vue'),
      props: (route) => {
        return {
          chatId: toNumberOrUndefined(route.params.chatId),
          newChatToUserId: toNumberOrUndefined(route.query.newChatToUserId),
          newChatToAdvertisementId: toNumberOrUndefined(route.query.newChatToAdvertisementId)
        }
      },
      meta: {
        requiresPermission: Permissions.ViewMessages,
        containerClass: 'h-0'
      }
    },

    //User advertisement notification subscription
    {
      path: '/advertisement-notification-subscriptions',
      redirect: { name: 'manageAdvertisementNotificationSubscription' },
      children: [
        {
          path: 'manage',
          name: 'manageOwnAdvertisementNotificationSubscription',
          component: () =>
            import('../views/advertisement-notification-subscriptions/ManageSubscriptions.vue'),
          meta: {
            requiresPermission: Permissions.ViewOwnedAdvertisementNotificationSubscriptions
          }
        },
        {
          path: 'manage-all',
          name: 'manageAdvertisementNotificationSubscription',
          component: () =>
            import('../views/advertisement-notification-subscriptions/ManageSubscriptions.vue'),
          props: () => ({ canManageAll: true }),
          meta: {
            requiresPermission: Permissions.ViewAllAdvertisementNotificationSubscriptions
          }
        },
        {
          path: 'create-own',
          name: 'createOwnAdvertisementNotificationSubscription',
          component: () =>
            import(
              '../views/advertisement-notification-subscriptions/CreateOrEditSubscription.vue'
            ),
          meta: {
            requiresPermission: Permissions.CreateOwnedAdvertisementNotificationSubscription
          }
        },
        {
          path: 'create',
          name: 'createAdvertisementNotificationSubscription',
          component: () =>
            import(
              '../views/advertisement-notification-subscriptions/CreateOrEditSubscription.vue'
            ),
          props: () => ({ forAnyUser: true }),
          meta: {
            requiresPermission: Permissions.CreateAdvertisementNotificationSubscription
          }
        },
        {
          path: 'view/:subscriptionId',
          name: 'viewAdvertisementNotificationSubscription',
          component: () =>
            import('../views/advertisement-notification-subscriptions/ViewSubscription.vue'),
          props: (route) => ({ subscriptionId: toNumberOrUndefined(route.params.subscriptionId) }),
          meta: {
            requiresPermission: Permissions.ViewOwnedAdvertisementNotificationSubscriptions
          }
        },
        {
          path: 'view-any/:subscriptionId',
          name: 'viewAnyAdvertisementNotificationSubscription',
          component: () =>
            import('../views/advertisement-notification-subscriptions/ViewSubscription.vue'),
          props: (route) => ({
            subscriptionId: toNumberOrUndefined(route.params.subscriptionId),
            forAnyUser: true
          }),
          meta: {
            requiresPermission: Permissions.ViewAllAdvertisementNotificationSubscriptions
          }
        },
        {
          path: 'edit/:subscriptionId',
          name: 'editAdvertisementNotificationSubscription',
          component: () =>
            import(
              '../views/advertisement-notification-subscriptions/CreateOrEditSubscription.vue'
            ),
          props: (route) => ({ subscriptionId: toNumberOrUndefined(route.params.subscriptionId) }),
          meta: {
            requiresPermission: Permissions.EditOwnedAdvertisementNotificationSubscriptions
          }
        },
        {
          path: 'edit-any/:subscriptionId',
          name: 'editAnyAdvertisementNotificationSubscription',
          component: () =>
            import(
              '../views/advertisement-notification-subscriptions/CreateOrEditSubscription.vue'
            ),
          props: (route) => ({
            subscriptionId: toNumberOrUndefined(route.params.subscriptionId),
            forAnyUser: true
          }),
          meta: {
            requiresPermission: Permissions.EditAnyAdvertisementNotificationSubscription
          }
        }
      ]
    },

    {
      path: '/attribute-value-list',
      children: [
        {
          path: 'manage',
          name: 'manageAttributeValueLists',
          component: () => import('../views/attribute-value-list/ManageAttributeValueLists.vue'),
          meta: {
            requiresPermission: Permissions.ViewAllAttributeValueLists
          }
        },
        {
          path: 'create',
          name: 'createAttributeValueList',
          component: () => import('../views/attribute-value-list/AttributeValueListForm.vue'),
          meta: {
            requiresPermission: Permissions.CreateAttributeValueList
          }
        },
        {
          path: 'view/:valueListId',
          name: 'viewAttributeValueList',
          component: () => import('../views/attribute-value-list/ViewAttributeValueList.vue'),
          props: (route) => ({ valueListId: toNumberOrUndefined(route.params.valueListId) }),
          meta: {
            requiresPermission: Permissions.ViewAllAttributeValueLists
          }
        },
        {
          path: 'edit/:valueListId',
          name: 'editAttributeValueList',
          component: () => import('../views/attribute-value-list/AttributeValueListForm.vue'),
          props: (route) => ({ valueListId: toNumberOrUndefined(route.params.valueListId) }),
          meta: {
            requiresPermission: Permissions.EditAttributeValueList
          }
        }
      ]
    },

    //User payments
    {
      path: '/payments',
      redirect: { name: 'viewPayments' },
      children: [
        {
          path: 'pay',
          name: 'makePayment',
          component: () => import('../views/payments/MakePayment.vue'),
          meta: {
            requiresPermission: Permissions.MakePayment
          }
        },
        {
          path: 'view',
          name: 'viewPayments',
          component: () => import('../views/payments/ViewPayments.vue'),
          meta: {
            requiresPermission: Permissions.ViewOwnPayments
          }
        },
        {
          path: 'view-system-payments',
          name: 'viewSystemPayments',
          component: () => import('../views/payments/ViewSystemPayments.vue'),
          meta: {
            requiresPermission: Permissions.ViewSystemPayments
          }
        },
        {
          path: 'view/:paymentId',
          name: 'viewPayment',
          component: () => import('../views/payments/ViewPayment.vue'),
          props: (route) => ({ paymentId: toNumberOrUndefined(route.params.paymentId) }),
          meta: {
            requiresPermission: Permissions.ViewOwnPayments
          }
        },
        {
          path: 'view-system-payment/:paymentId',
          name: 'viewSystemPayment',
          component: () => import('../views/payments/ViewPayment.vue'),
          props: (route) => ({
            paymentId: toNumberOrUndefined(route.params.paymentId),
            canViewAnyPayment: true
          }),
          meta: {
            requiresPermission: Permissions.ViewSystemPayments
          }
        }
      ]
    },

    //Category
    {
      path: '/categories',
      redirect: { name: 'manageCategories' },
      children: [
        {
          path: 'view',
          name: 'manageCategories',
          component: () => import('../views/categories/ManageCategories.vue'),
          meta: {
            requiresPermission: Permissions.ViewCategories
          }
        },
        {
          path: 'view/:categoryId',
          name: 'viewCategory',
          component: () => import('../views/categories/ViewCategory.vue'),
          props: (route) => ({
            categoryId: toNumberOrUndefined(route.params.categoryId)
          }),
          meta: {
            requiresPermission: Permissions.ViewCategories
          }
        },
        {
          path: 'create/:parentCategoryId?',
          name: 'createCategory',
          component: () => import('../views/categories/CategoryForm.vue'),
          props: (route) => ({
            parentCategoryId: toNumberOrUndefined(route.params.parentCategoryId)
          }),
          meta: {
            requiresPermission: Permissions.CreateCategory
          }
        },
        {
          path: 'edit/:categoryId',
          name: 'editCategory',
          component: () => import('../views/categories/CategoryForm.vue'),
          props: (route) => ({ categoryId: toNumberOrUndefined(route.params.categoryId) }),
          meta: {
            requiresPermission: Permissions.EditCategory
          }
        }
      ]
    },

    //Attributes
    {
      path: '/attributes/',
      children: [
        {
          path: 'view',
          name: 'manageAttributes',
          component: () => import('../views/attributes/ManageAttributes.vue'),
          meta: {
            requiresPermission: Permissions.ViewAllAttributes
          }
        },
        {
          path: 'create',
          name: 'createAttribute',
          component: () => import('../views/attributes/AttributeForm.vue'),
          meta: {
            requiresPermission: Permissions.CreateAttribute
          }
        },
        {
          path: 'view/:attributeId',
          name: 'viewAttribute',
          component: () => import('../views/attributes/ViewAttribute.vue'),
          props: (route) => ({ attributeId: toNumberOrUndefined(route.params.attributeId) }),
          meta: {
            requiresPermission: Permissions.ViewAllAttributes
          }
        },
        {
          path: 'edit/:attributeId',
          name: 'editAttribute',
          component: () => import('../views/attributes/AttributeForm.vue'),
          props: (route) => ({ attributeId: toNumberOrUndefined(route.params.attributeId) }),
          meta: {
            requiresPermission: Permissions.EditAttribute
          }
        }
      ]
    },

    //Roles
    {
      path: '/roles',
      children: [
        {
          path: 'manage',
          name: 'manageRoles',
          component: () => import('../views/roles/ManageRoles.vue'),
          meta: {
            requiresPermission: Permissions.ViewAllRoles
          }
        },
        {
          path: 'create',
          name: 'createRole',
          component: () => import('../views/roles/RoleForm.vue'),
          meta: {
            requiresPermission: Permissions.AddRole
          }
        },
        {
          path: 'edit/:roleId',
          name: 'editRole',
          component: () => import('../views/roles/RoleForm.vue'),
          props: (route) => ({ roleId: toNumberOrUndefined(route.params.roleId) }),
          meta: {
            requiresPermission: Permissions.EditRole
          }
        },
        {
          path: 'view/:roleId',
          name: 'viewRole',
          component: () => import('../views/roles/ViewRole.vue'),
          props: (route) => ({ roleId: toNumberOrUndefined(route.params.roleId) }),
          meta: {
            requiresPermission: Permissions.ViewAllRoles
          }
        }
      ]
    },

    //Terms of use
    {
      path: '/terms-of-use',
      name: 'viewTermsOfUse',
      component: () => import('../views/ViewTermsOfUse.vue')
    },

    //Service prices
    {
      path: '/manage-service-prices',
      name: 'manageServicePrices',
      component: () => import('../views/ManageServicePrices.vue'),
      meta: {
        requiresPermission: Permissions.ManageServicePrices
      }
    },

    //Permissions
    {
      path: '/manage-permissions',
      name: 'managePermissions',
      component: () => import('../views/ManagePermissions.vue'),
      meta: {
        requiresPermission: Permissions.ViewAllPermissions
      }
    },

    //Rule violation reports
    {
      path: '/rule-violation-reports',
      redirect: { name: 'manageRuleViolationReports' },
      children: [
        {
          path: 'manage',
          name: 'manageRuleViolationReports',
          component: () => import('../views/rule-violation-reports/ManageRuleViolationReports.vue'),
          meta: {
            requiresPermission: Permissions.ViewRuleViolationReports
          }
        },
        {
          path: 'view/:id',
          name: 'viewRuleViolationReport',
          component: () => import('../views/rule-violation-reports/ViewRuleViolationReport.vue'),
          props: (route) => ({ id: toNumberOrUndefined(route.params.id) })
        }
      ]
    },

    //Not found
    {
      path: '/:all(.*)*',
      name: 'notFound',
      component: () => import('../views/NotFound.vue')
    }
  ]
})

//Add route permission guard
router.beforeEach(async (to, _, next) => {
  const requiresPermission = to.meta?.requiresPermission
  if (requiresPermission) {
    await AuthService.permissionsPromise.value
    if (AuthService.hasPermission(requiresPermission as Permissions)) {
      next()
    } else {
      if (AuthService.isAuthenticated.value) {
        next({ name: 'notFound' })
      } else {
        next({ name: 'login', query: { redirect: 'true' } })
      }
    }
  } else {
    next()
  }
})

const firstParam = (p: string | string[]) => {
  return Array.isArray(p) ? p[0] : p
}

const toNumberOrUndefined = (
  param: string | string[] | LocationQueryValue | LocationQueryValue[]
) => {
  const firstParam = Array.isArray(param) ? param[0] : param
  if (typeof firstParam !== 'string') {
    return undefined
  }

  const numberParam = parseInt(firstParam)
  return isNaN(numberParam) ? undefined : numberParam
}

export default router
