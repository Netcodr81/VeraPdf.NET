/*
 * This file is part of veraPDF Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.pd.annotations;

import org.verapdf.as.ASAtom;
import org.verapdf.cos.COSKey;
import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSObject;
import org.verapdf.cos.COSString;
import org.verapdf.gf.model.impl.containers.StaticContainers;
import org.verapdf.gf.model.impl.pd.GFPDAnnot;
import org.verapdf.gf.model.impl.pd.GFPDDestination;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.pdlayer.PDDestination;
import org.verapdf.model.pdlayer.PDLinkAnnot;
import org.verapdf.pd.PDAnnotation;
import org.verapdf.pd.PDNameTreeNode;
import org.verapdf.pd.PDNamesDictionary;
import org.verapdf.pd.PDPage;
import org.verapdf.pd.actions.PDAction;
import org.verapdf.pdfa.flavours.PDFFlavours;
import org.verapdf.tools.StaticResources;

import java.util.*;
import java.util.Map.Entry;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Adapts a link annotation, including its destination and tagged-PDF destination checks.
 *
 * <p>Link annotations connect a region on a page to a destination or action. Destinations
 * can be direct arrays, legacy name keys, or strings resolved through the catalog name
 * tree. For PDF/UA-2-related flavours, this class also resolves structure destinations
 * and records whether annotations sharing a structure parent target different structure
 * elements. This bookkeeping feeds accessibility profile rules; it is performed only
 * for the matching flavour and does not follow the link interactively.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested type: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Annotations.GFPDLinkAnnot},
 *       implementing {@code VeraPdf.Model.PDLayer.IPDLinkAnnot}.</li>
 *   <li>Model the destination as {@code IReadOnlyList<IPDDestination>}; resolve names
 *       through {@code PdfCatalog} and {@code CosDictionary}/{@code CosArray} adapters.</li>
 *   <li>The static container map is shared mutable state keyed by indirect references;
 *       prefer a per-document context in C# and preserve key equality semantics. Map
 *       Java logging to {@code ILogger<GFPDLinkAnnot>}.</li>
 * </ul>
 *
 * @author Maxim Plushchov
 */
public class GFPDLinkAnnot extends GFPDAnnot implements PDLinkAnnot {

	/** Logger for unresolved named destinations encountered during model construction. */
	public static final Logger LOGGER = Logger.getLogger(GFPDLinkAnnot.class.getCanonicalName());

	/** Model type name for a link annotation. */
	public static final String LINK_ANNOTATION_TYPE = "PDLinkAnnot";

	/** Link name used by profile rules to reach an explicit destination. */
	public static final String DEST = "Dest";

	/** Indirect key of the structure destination resolved for this annotation. */
	private COSKey structDestinationKey = null;

	/** First other target registered for the same structure parent, if any. */
	private Entry<COSKey, String> differentTargetAnnot = null;

	/** Initializes the annotation and computes structure-target data for PDF/UA-2. */
	public GFPDLinkAnnot(PDAnnotation annot, PDResourcesHandler pageResources, PDPage page) {
		super(annot, pageResources, page, LINK_ANNOTATION_TYPE);
		if (PDFFlavours.isPDFUA2RelatedFlavour(StaticContainers.getFlavour())) {
			calculateStructDestinationProperties();
		}
	}

	@Override
	/** Resolves the model's {@code Dest} link and delegates standard annotation links. */
	public List<? extends Object> getLinkedObjects(String link) {
		switch (link) {
			case DEST:
				return this.getDestination();
			default:
				return super.getLinkedObjects(link);
		}
	}

	/** Returns an explicit destination only when the annotation has no {@code /A} action. */
	private List<PDDestination> getDestination() {
		COSObject destination = ((PDAnnotation) simplePDObject).getDestination();
		if (!destination.empty() && !simplePDObject.knownKey(ASAtom.A)) {
			List<PDDestination> destinations = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
			destinations.add(new GFPDDestination(new org.verapdf.pd.PDDestination(destination)));
			return Collections.unmodifiableList(destinations);
		}
		return Collections.emptyList();
	}

	/** Registers structure-destination targets to detect conflicting annotations per parent. */
	private void calculateStructDestinationProperties() {
		COSObject parent = getParentDictionary();
		if (parent == null || parent.getKey() == null) {
			return;
		}
		COSKey structParentKey = parent.getKey();
		COSObject structDestination = getStructureDestinationObject();
		if (structDestination == null || structDestination.getKey() == null) {
			return;
		}
		structDestinationKey = structDestination.getKey();
		Map<COSKey, String> destinationToAnnotMap = StaticContainers.getStructParentToDestinationToAnnotMap()
				.computeIfAbsent(structParentKey, k -> new HashMap<>());
		for (Map.Entry<COSKey, String> entry : destinationToAnnotMap.entrySet()) {
			if (!structDestinationKey.equals(entry.getKey())) {
				differentTargetAnnot = entry;
				break;
			}
		}
		StaticContainers.getStructParentToDestinationToAnnotMap().get(structParentKey).put(structDestinationKey, getobjectKey());
	}
	
	/** Resolves a direct or named destination to its structure-destination dictionary. */
	private COSObject getStructureDestinationObject() {
		COSObject destination = simpleCOSObject;
		if (simpleCOSObject.knownKey(ASAtom.A)) {
			PDAction action = ((PDAnnotation) simplePDObject).getA();
			if (action != null && ASAtom.GO_TO.equals(action.getSubtype())) {
				destination = action.knownKey(ASAtom.SD) ? action.getStructureDestination() : action.getDestination();
			}
		} else if (simpleCOSObject.knownKey(ASAtom.DEST)) {
			destination = ((PDAnnotation) simplePDObject).getDestination();
		}
		if (destination == null) {
			return null;
		}
		if (destination.getType() == COSObjType.COS_STRING) {
			PDNamesDictionary namesDictionary = StaticResources.getDocument().getCatalog().getNamesDictionary();
			if (namesDictionary == null) {
				return null;
			}
			PDNameTreeNode dests = namesDictionary.getDests();
			if (dests != null) {
				COSObject dest = dests.getObject((COSString) destination.getDirectBase());
				if (dest == null) {
					LOGGER.log(Level.WARNING, "Named destination " + destination.getString() + 
							" not found in the Dests name tree in the Names dictionary");
					return null;
				}
				destination = dest;
			}
		} else if (destination.getType() == COSObjType.COS_NAME) {
			COSObject dests = StaticResources.getDocument().getCatalog().getDests();
			if (dests != null) {
				COSObject dest = dests.getKey(destination.getName());
				if (dest == null) {
					LOGGER.log(Level.WARNING, "Named destination " + destination.getName() + 
							" not found in the Dests dictionary in the catalog");
					return null;
				}
				destination = dest;
			}
		}
		if (destination.getType() == COSObjType.COS_DICT) {
			return destination.getKey(ASAtom.SD);
		}
		if (destination.getType() == COSObjType.COS_ARRAY && destination.size() > 0) {
			destination = destination.at(0);
			if (destination != null && destination.getType() == COSObjType.COS_DICT && destination.knownKey(ASAtom.S)) {
				return destination;
			}
		}
		return null;
	}

	@Override
	/** Returns the resolved structure destination's indirect key, if available. */
	public String getstructDestinationKey() {
		if (structDestinationKey == null) return null;
		return structDestinationKey.toString();
	}

	@Override
	/** Returns the other annotation's object key when a different target was recorded. */
	public String getdifferentTargetAnnotObjectKey() {
		if (differentTargetAnnot == null) return null;
		return differentTargetAnnot.getValue();
	}

	@Override
	/** Returns the different target's destination key, if one was recorded. */
	public String getdifferentTargetAnnotDestinationKey() {
		if (differentTargetAnnot == null) return null;
		return differentTargetAnnot.getKey().toString();
	}
}
