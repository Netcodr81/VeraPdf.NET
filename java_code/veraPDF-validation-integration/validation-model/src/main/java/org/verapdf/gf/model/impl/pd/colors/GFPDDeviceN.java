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
package org.verapdf.gf.model.impl.pd.colors;

import org.verapdf.as.ASAtom;
import org.verapdf.cos.*;
import org.verapdf.gf.model.factory.colors.ColorSpaceFactory;
import org.verapdf.gf.model.factory.functions.FunctionFactory;
import org.verapdf.gf.model.impl.pd.functions.GFPDFunction;
import org.verapdf.gf.model.impl.cos.GFCosUnicodeName;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.coslayer.CosUnicodeName;
import org.verapdf.model.pdlayer.PDColorSpace;
import org.verapdf.model.pdlayer.PDDeviceN;
import org.verapdf.model.pdlayer.PDSeparation;
import org.verapdf.pd.function.PDFunction;

import java.util.*;

/**
 * Models a DeviceN color space with multiple named colorants and conversion links.
 *
 * <p>DeviceN represents one or more named colorants (often spot inks) and may provide
 * an alternate color space, a tint-transform function, process-color information, and
 * individual Separation colorants. The constructor also checks that non-process
 * colorants are represented by the attributes dictionary, excluding standard process
 * color names and {@code /None}. These links support PDF/A color-conversion rules.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested type: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Colors.GFPDDeviceN},
 *       implementing {@code VeraPdf.Model.PDLayer.IPDDeviceN}.</li>
 *   <li>Use {@code IReadOnlyList<T>}, {@code HashSet<ASAtom>} / a typed name set, and
 *       explicit subtype adapters from {@code ColorSpaceFactory} and {@code FunctionFactory}.</li>
 *   <li>Preserve ordered component names, null/empty distinctions, and the set of
 *       ignored process colorants; do not assume component-name objects are all valid.</li>
 * </ul>
 *
 * @author Maksim Bezrukov
 */
public class GFPDDeviceN extends GFPDColorSpace implements PDDeviceN {

	/** Model type name for a DeviceN color space. */
	public static final String DEVICE_N_TYPE = "PDDeviceN";

	/** Link name for the DeviceN tint-transform function. */
	public static final String TINT_TRANSFORM = "tintTransform";
	/** Link name for the alternate color space. */
	public static final String ALTERNATE = "alternate";
	/** Link name for the ordered colorant names. */
	public static final String COLORANT_NAMES = "colorantNames";
	/** Link name for individual Separation colorant spaces. */
	public static final String COLORANTS = "Colorants";
	/** Link name for process-color information. */
	public static final String PROCESS_COLOR = "processColor";

	/** Process colors expected to be described separately, not repeated in Colorants. */
	public static final Set<ASAtom> IGNORED_COLORANTS;

	static {
		Set<ASAtom> tempIgnore = new HashSet<>();
		tempIgnore.add(ASAtom.getASAtom("Cyan"));
		tempIgnore.add(ASAtom.getASAtom("Magenta"));
		tempIgnore.add(ASAtom.getASAtom("Yellow"));
		tempIgnore.add(ASAtom.getASAtom("Black"));
		IGNORED_COLORANTS = Collections.unmodifiableSet(tempIgnore);
	}

	/** Computed once because the test depends only on the source DeviceN attributes. */
	private final boolean areColorantsPresent;

	/** Wraps DeviceN and validates listed colorants against attributes/process entries. */
	public GFPDDeviceN(org.verapdf.pd.colors.PDDeviceN simplePDObject) {
		super(simplePDObject, DEVICE_N_TYPE);
		this.areColorantsPresent = GFPDDeviceN.areColorantsPresent(simplePDObject);
	}

	/** Collects colorant and process component names before checking consistency. */
	private static boolean areColorantsPresent(org.verapdf.pd.colors.PDDeviceN simplePDObject) {
		COSObject attributes = simplePDObject.getAttributes();
		List<COSObject> colorantsArray = simplePDObject.getNames();
		if (attributes != null && attributes.getType() == COSObjType.COS_DICT) {
			COSObject colorantsDict = attributes.getKey(ASAtom.COLORANTS);
			Set<ASAtom> componentNames = new HashSet<>();
			if (colorantsDict != null && colorantsDict.getType() == COSObjType.COS_DICT) {
				componentNames.addAll(colorantsDict.getKeySet());
			}
			COSArray components = getProcessComponents(attributes);
			if (components != null) {
				for (COSObject component : components) {
					if (component.getType() == COSObjType.COS_NAME) {
						componentNames.add(component.getName());
					}
				}
			}
			return GFPDDeviceN.areColorantsPresent(componentNames, colorantsArray);
		}
		return GFPDDeviceN.areColorantsPresent(Collections.emptySet(), colorantsArray);
	}

	/** Returns {@code /Process /Components} only when it is a non-empty array. */
	private static COSArray getProcessComponents(COSObject attributes) {
		COSObject process = attributes.getKey(ASAtom.PROCESS);
		if (!process.empty()) {
			COSObject components = process.getKey(ASAtom.COMPONENTS);
			if (!components.empty() && components.getType() == COSObjType.COS_ARRAY) {
				return (COSArray) components.getDirectBase();
			}
		}
		return null;
	}

	/** Checks each declared colorant while allowing {@code /None} and process inks. */
	private static boolean areColorantsPresent(Set<ASAtom> colorantDictionaryEntries,
	                                           List<COSObject> colorantsArray) {
		for (COSObject object : colorantsArray) {
			if (object != null && !isNone(object) && !IGNORED_COLORANTS.contains(object.getName()) &&
			    !colorantDictionaryEntries.contains(object.getName())) {
				return false;
			}
		}
		return true;
	}

	/** Identifies the special {@code /None} colorant name. */
	private static boolean isNone(COSObject obj) {
		return obj.getType() == COSObjType.COS_NAME && obj.getName() == ASAtom.NONE;
	}

	@Override
	/** Returns the constructor's colorant-presence consistency result. */
	public Boolean getareColorantsPresent() {
		return this.areColorantsPresent;
	}

	@Override
	/** Dispatches the color-space links defined by the DeviceN model contract. */
	public List<? extends Object> getLinkedObjects(String link) {
		switch (link) {
			case ALTERNATE:
				return this.getAlternate();
			case COLORANT_NAMES:
				return this.getColorantNames();
			case COLORANTS:
				return this.getColorants();
			case PROCESS_COLOR:
				return this.getProcessColor();
			case TINT_TRANSFORM:
				return this.getTintTransform();
			default:
				return super.getLinkedObjects(link);
		}
	}

	/** Wraps the optional process color space as a model link. */
	private List<PDColorSpace> getProcessColor() {
		org.verapdf.pd.colors.PDColorSpace colorSpace = ((org.verapdf.pd.colors.PDDeviceN) this.simplePDObject).getProcessColorSpace();
		if (colorSpace == null) {
			return Collections.emptyList();
		}

		return Collections.singletonList(ColorSpaceFactory.getColorSpace(colorSpace));
	}

	/** Wraps the alternate color space used to approximate DeviceN output. */
	private List<PDColorSpace> getAlternate() {
		org.verapdf.pd.colors.PDColorSpace alternateColorSpace = ((org.verapdf.pd.colors.PDDeviceN) this.simplePDObject)
				.getAlternateSpace();
		PDColorSpace space = ColorSpaceFactory.getColorSpace(alternateColorSpace);
		if (space != null) {
			List<PDColorSpace> colorSpace = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
			colorSpace.add(space);
			return Collections.unmodifiableList(colorSpace);
		}
		return Collections.emptyList();
	}

	/** Converts valid COS names to Unicode-name model objects, preserving their order. */
	private List<CosUnicodeName> getColorantNames() {
		List<COSObject> colorants = ((org.verapdf.pd.colors.PDDeviceN) this.simplePDObject).getNames();
		if (!colorants.isEmpty()) {
			List<CosUnicodeName> list = new ArrayList<>(colorants.size());
			for (COSObject colorant : colorants) {
				if (colorant.getType() == COSObjType.COS_NAME) {
					list.add(new GFCosUnicodeName((COSName) colorant.getDirectBase()));
				}
			}
			return Collections.unmodifiableList(list);
		}
		return Collections.emptyList();
	}

	/** Keeps only colorant entries represented by a Separation color space. */
	private List<PDSeparation> getColorants() {
		List<PDSeparation> result = new LinkedList<>();
		List<org.verapdf.pd.colors.PDColorSpace> colorants = ((org.verapdf.pd.colors.PDDeviceN) this.simplePDObject).getColorants();
		for (org.verapdf.pd.colors.PDColorSpace colorSpace : colorants) {
			if (colorSpace != null && ASAtom.SEPARATION.equals(colorSpace.getType())) {
				result.add((GFPDSeparation) ColorSpaceFactory.getColorSpace(colorSpace));
			}
		}
		return result;
	}

	/** Creates the specialized function adapter for the optional tint transform. */
	private List<GFPDFunction> getTintTransform() {
		PDFunction pdFunction = ((org.verapdf.pd.colors.PDDeviceN) this.simplePDObject).getTintTransform();
		if (pdFunction == null) {
			return Collections.emptyList();
		}
		GFPDFunction gfpdFunction = FunctionFactory.createFunction(pdFunction);
		return Collections.singletonList(gfpdFunction);
	}
}
