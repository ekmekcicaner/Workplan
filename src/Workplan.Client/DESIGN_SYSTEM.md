# Workplan Client Design System

Workplan uses a **warm industrial** visual language: graphite typography, warm neutral surfaces and a controlled orange accent. Orange communicates primary action and active navigation; operational state is represented by semantic success, warning, danger and info colors.

## Layers

- `Components/Ui`: domain-free primitives. They must not inject API services or depend on Workplan DTOs.
- `Components/Patterns`: reusable page and data-display compositions.
- Feature folders: domain-specific forms, cards, timelines and panels.
- `Pages`: route, authorization, API loading and page-level state ownership.

Data flows down through parameters. User actions flow up through `EventCallback<T>`. Collection parameters use `IReadOnlyList<T>` and repeated elements require a stable key.

## Tokens

Semantic colors are exposed as Tailwind utilities (`bg-canvas`, `bg-surface`, `text-foreground`, `text-foreground-muted`, `border-border`, `text-success`, and related soft backgrounds). Their light and dark values live in `Styles/theme.css`; components should prefer semantic utilities over direct neutral color values.

Controls use a 44px minimum target. The radius hierarchy is control (10px), panel (14px), dialog (18px). Motion is short and functional, and must respect `prefers-reduced-motion`.

## Components

- Use `WpButton` and `WpIconButton` when the component owns loading/disabled behavior or a stable variant. Native links and buttons may use the shared button classes for simple one-off markup.
- Use `WpSurface` for titled content regions, `WpField` for label/help/validation composition, and `WpBadge` for semantic labels.
- Use `WpResponsiveDataView<TItem>` for list-based API results: QuickGrid on desktop and a feature-supplied card template on mobile.
- Use `WpModal` for short decisions and `WpDrawer` for longer inspection/edit flows.

Do not create a generic component for a single use. Extract repeated markup first, promote repeated values to tokens, and add component CSS primarily for stable primitives or generated markup such as QuickGrid.

## QuickGrid

Current endpoints return complete lists, so existing screens use local `IQueryable<T>` sorting and pagination. A future endpoint may use `ItemsProvider` only when it supports skip/take, sorting and total count. Pagination and virtualization are not combined; virtualization requires constant row height.
