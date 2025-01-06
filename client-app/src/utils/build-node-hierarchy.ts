/**
 * Builds hierarchical object structure from flat object array.
 * @param dtos array of flat dtos
 * @param key key of dto identifier
 * @param parentKey key of dto parent identifier
 * @param mappingFunction function which will map dto to hierarchy node
 * @param transformFunction function to modify nodes after mapping
 */
export const buildNodeHierarchy = <InputDto, HierarchyNode>(
  dtos: InputDto[],
  key: keyof InputDto,
  parentKey: keyof InputDto,
  mappingFunction: (dto: InputDto, children: HierarchyNode[]) => HierarchyNode,
  transformFunction?: (node: HierarchyNode) => void
) => {
  const buildBranch = (parentKeyValue: unknown): HierarchyNode[] => {
    const branchRootDtos = dtos.filter((dto) => dto[parentKey] === parentKeyValue)
    if (!branchRootDtos.length) {
      return []
    }

    const branchNodes: HierarchyNode[] = []
    for (const rootDto of branchRootDtos) {
      const children = buildBranch(rootDto[key])
      const node = mappingFunction(rootDto, children)
      transformFunction?.(node)
      branchNodes.push(node)
    }
    return branchNodes
  }

  return buildBranch(null)
}
