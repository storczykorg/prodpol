import type { Router } from "vue-router";
import { type Component, computed } from "vue";
import DefaultLayout from "./DefaultLayout.vue";
import NavLayout from "./NavLayout.vue";
import AdminLayout from "./AdminLayout.vue";

export function useLayoutStrategy(router: Router): Component {
  return computed(() => {
    const current = router.currentRoute.value;

    if (current.matched
      .some(matched => Boolean(matched.meta["noNavigation"]))) {
      return DefaultLayout;
    }
    if (current.matched.some(matched => matched.meta["adminNav"])) {
      return AdminLayout;
    }
    return NavLayout;
  });
}
