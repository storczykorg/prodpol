<script lang="ts" setup>
import "./style.css";

import { useRouter} from "vue-router"
import {layoutStrategy} from "./layouts";

const router = useRouter()

const layout = layoutStrategy(router)

</script>

<template>
  <RouterView v-slot="{ Component }">
    <component :is="layout" v-if="Component">
      <Transition mode="out-in">
        <KeepAlive>
          <Suspense>
            <component :is="Component"></component>
            <template #fallback>
              Loading...
            </template>
          </Suspense>
        </KeepAlive>
      </Transition>
    </component>
    <component :is="layout" v-else/>
  </RouterView>
</template>