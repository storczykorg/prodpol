<script lang="ts" setup>
import "./style.css";

import { useRouter} from "vue-router"
import {useLayoutStrategy} from "./layouts";

const router = useRouter()

const layout = useLayoutStrategy(router)

</script>

<template>
  <RouterView v-slot="{ Component }">
    <component :is="layout" v-if="Component">
      <KeepAlive>
        <Suspense>
          <component :is="Component"></component>
          <template #fallback>
            Loading...
          </template>
        </Suspense>
      </KeepAlive>
    </component>
    <component :is="layout" v-else/>
  </RouterView>
</template>