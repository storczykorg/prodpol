import type { Router } from "vue-router";
import type { Component } from "vue";
import DefaultLayout from "./DefaultLayout.vue";
import NavLayout from "./NavLayout.vue";

export function layoutStrategy(router: Router): Component {
  const current = router.currentRoute.value;

  if (current.meta["noNavigation"]) {
    return DefaultLayout;
  }
  return NavLayout;
}
